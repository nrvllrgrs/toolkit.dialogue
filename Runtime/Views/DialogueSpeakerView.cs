using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToolkitEngine.Dialogue;
using UnityEngine;

namespace Yarn.Unity
{
	/// <summary>
	/// A subclass of <see cref="DialogueViewBase"/> that plays voice-over <see
	/// cref="AudioClip"/>s for lines of dialogue.
	/// </summary>
	/// <remarks>
	/// This class plays audio clip assets that are provided by an <see
	/// cref="AudioLineProvider"/>. To use a <see cref="DialogueSpeakerView"/> in your
	/// game, your <see cref="DialogueRunner"/> must be configured to use an
	/// <see cref="AudioLineProvider"/>, and your Yarn projects must be
	/// configured to use voice-over audio assets. For more information, see
	/// [Localization and Assets](/docs/using-yarnspinner-with-unity/assets-and-localization/README.md).
	/// </remarks>
	/// <seealso cref="DialogueViewBase"/>
	public class DialogueSpeakerView : DialogueViewBase
	{
		#region Fields

		/// <summary>
		/// The fade out time when <see cref="UserRequestedViewAdvancement"/> is
		/// called.
		/// </summary>
		public float fadeOutTimeOnLineFinish = 0.05f;

		/// <summary>
		/// The amount of time to wait before starting playback of the line.
		/// </summary>
		public float waitTimeBeforeLineStart = 0f;

		/// <summary>
		/// The amount of time after playback has completed before this view
		/// reports that it's finished delivering the line.
		/// </summary>
		public float waitTimeAfterLineComplete = 0f;

		/// <summary>
		/// The <see cref="AudioSource"/> that this voice over view will play
		/// its audio from.
		/// </summary>
		/// <remarks>If this is <see langword="null"/>, a new <see
		/// cref="AudioSource"/> will be added at runtime.</remarks>
		[SerializeField]
		private AudioSource m_audioSource;

		/// <summary>
		/// List of speakers involved in this conversation
		/// </summary>
		[SerializeField]
		private List<DialogueSpeaker> m_speakers;

		/// <summary>
		/// The current coroutine that's playing a line.
		/// </summary>
		private Coroutine m_playbackCoroutine;

		/// <summary>
		/// An interrupt token that can be used to interrupt <see
		/// cref="m_playbackCoroutine"/>.
		/// </summary>
		private Effects.CoroutineInterruptToken m_interruptToken = new Effects.CoroutineInterruptToken();

		/// <summary>
		/// The method that should be called before <see
		/// cref="m_playbackCoroutine"/> exits.
		/// </summary>
		/// <remarks>
		/// This value is set by <see cref="RunLine"/> and <see
		/// cref="InterruptLine"/>.
		/// </remarks>
		private Action m_completionHandler;

		private Dictionary<string, DialogueSpeaker> m_map = new();

		private HashSet<AudioSource> m_activeAudioSources = new();
		private float m_volume = 0f;

		#endregion

		#region Properties

		private bool isTracked => m_activeAudioSources.Count > 0;

		private float volume
		{
			get => m_volume;
			set
			{
				// No change, skip
				if (m_volume == value)
					return;

				m_volume = value;
				foreach (var audioSource in m_activeAudioSources)
				{
					audioSource.volume = value;
				}
			}
		}

		#endregion

		#region Methods

		private void Awake()
		{
			if (m_audioSource == null)
			{
				// If we don't have an audio source, add one. 
				m_audioSource = gameObject.AddComponent<AudioSource>();

				// Additionally, we'll assume that the user didn't place the
				// game object that this component is attached to deliberately,
				// so we'll set the spatial blend to 0 (which means the audio
				// will not be positioned in 3D space.)
				m_audioSource.spatialBlend = 0f;
			}

			foreach (var speaker in m_speakers)
			{
				if (!m_map.ContainsKey(speaker.speakerType.characterName))
				{
					m_map.Add(speaker.speakerType.characterName, speaker);
				}
				else
				{
					Debug.LogError($"Speaker {speaker.speakerType.characterName} already exists! Cannot have speakers with the same name.");
					enabled = false;
				}
			}
		}

		/// <summary>
		/// Begins playing the associated audio for the specified line.
		/// </summary>
		/// <remarks>
		/// <para style="warning">This method is not intended to be called from
		/// your code. Instead, the <see cref="DialogueRunner"/> class will call
		/// it at the appropriate time.</para>
		/// </remarks>
		/// <inheritdoc cref="DialogueViewBase.RunLine(LocalizedLine, Action)"
		/// path="/param"/>
		/// <seealso cref="DialogueViewBase.RunLine(LocalizedLine, Action)"/>
		public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
		{
			// If we have a current playback for some reason, stop it
			// immediately.
			if (m_playbackCoroutine != null)
			{
				StopCoroutine(m_playbackCoroutine);
				Stop();
				m_playbackCoroutine = null;
			}

			// Set the handler to call when the line has finished presenting.
			// (This might change later, if the line gets interrupted.)
			m_completionHandler = onDialogueLineFinished;

			m_playbackCoroutine = StartCoroutine(DoRunLine(dialogueLine));
		}

		private IEnumerator DoRunLine(LocalizedLine dialogueLine)
		{
			// Get the localized voice over audio clip
			var voiceOverClip = dialogueLine.Asset as AudioClip;
			if (voiceOverClip == null)
			{
				Debug.LogError($"Playing voice over failed because the localised line {dialogueLine.TextID} either didn't have an asset, or its asset was not an {nameof(AudioClip)}.", gameObject);

				m_completionHandler?.Invoke();
				yield break;
			}

			if (isTracked)
			{
				// Usually, this shouldn't happen because the DialogueRunner
				// finishes and ends a line first
				Stop();
			}

			m_interruptToken.Start();

			// If we need to wait before starting playback, do this now
			if (waitTimeBeforeLineStart > 0)
			{
				var elaspedTime = 0f;
				while (elaspedTime < waitTimeBeforeLineStart)
				{
					if (m_interruptToken.WasInterrupted)
					{
						// We were interrupted in the middle of waiting to
						// start. Stop immediately before playing anything.
						m_completionHandler?.Invoke();
						yield break;
					}
					yield return null;
					elaspedTime += Time.deltaTime;
				}
			}

			// Start playing the audio.
			Play(dialogueLine, voiceOverClip);

			// Wait until either the audio source finishes playing, or the
			// interruption flag is set.
			while (IsAnyPlaying() && !m_interruptToken.WasInterrupted)
			{
				yield return null;
			}

			// If the line was interrupted, we need to wrap up the playback as
			// quickly as we can. We do this here with a fade-out to zero over
			// fadeOutTimeOnLineFinish seconds.
			if (m_interruptToken.WasInterrupted)
			{
				// Fade out voice over clip
				float lerpPosition = 0f;
				float volumeFadeStart = volume;
				while (volume != 0)
				{
					// We'll use unscaled time here, because if time is scaled,
					// we might be fading out way too slowly, and that would
					// sound extremely strange.
					lerpPosition += Time.unscaledDeltaTime / fadeOutTimeOnLineFinish;
					volume = Mathf.Lerp(volumeFadeStart, 0, lerpPosition);
					yield return null;
				}

				// We're done fading out. Restore our audio volume to its
				// original point for the next line.
				volume = volumeFadeStart;
			}
			Stop();

			// We've finished our playback at this point, either by waiting
			// normally or by interrupting it with a fadeout. If we weren't
			// interrupted, and we have additional time to wait after the audio
			// finishes, wait now. (If we were interrupted, we skip this wait,
			// because the user has already indicated that they're fine with
			// things moving faster than sounds normal.)

			if (!m_interruptToken.WasInterrupted && waitTimeAfterLineComplete > 0)
			{
				var elapsed = 0f;
				while (elapsed < waitTimeAfterLineComplete && !m_interruptToken.WasInterrupted)
				{
					yield return null;
					elapsed += Time.deltaTime;
				}
			}

			m_completionHandler?.Invoke();
			m_interruptToken.Complete();
		}

		/// <summary>
		/// Interrupts the playback of the specified line, and quickly fades the
		/// playback to silent.
		/// </summary>
		/// <inheritdoc cref="RunLine(LocalizedLine, Action)" path="/remarks"/>
		/// <inheritdoc cref="DialogueViewBase.InterruptLine(LocalizedLine,
		/// Action)" path="/param"/>
		/// <seealso cref="DialogueViewBase.InterruptLine(LocalizedLine,
		/// Action)"/>
		public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
		{
			if (m_interruptToken.CanInterrupt)
			{
				m_completionHandler = onDialogueLineFinished;
				m_interruptToken.Interrupt();
			}
			else
			{
				onDialogueLineFinished();
			}
		}

		/// <summary>
		/// Ends any existing playback, and reports that the line has finished
		/// dismissing.
		/// </summary>
		/// <inheritdoc cref="RunLine(LocalizedLine, Action)" path="/remarks"/>
		/// <inheritdoc cref="DialogueViewBase.DismissLine(Action)"
		/// path="/param"/>
		/// <seealso cref="DialogueViewBase.DismissLine(Action)"/>
		public override void DismissLine(Action onDismissalComplete)
		{
			// There's not much to do for a dismissal, since there's nothing
			// visible on screen and any audio playback is likely to have
			// finished as part of RunLine or InterruptLine completing. 

			// We'll stop the audio source, just to be safe, and immediately
			// report that we're done.
			Stop();
			onDismissalComplete();
		}

		/// <summary>
		/// Signals to this dialogue view that the user would like to skip
		/// playback.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When this method is called, this view indicates to its <see
		/// cref="DialogueRunner"/> that the line should be interrupted.
		/// </para>
		/// <para>
		/// If this view is not currently playing any audio, this method does
		/// nothing.
		/// </para>
		/// </remarks>
		/// <seealso cref="DialogueViewBase.InterruptLine(LocalizedLine, Action)"/>
		public override void UserRequestedViewAdvancement()
		{
			// We arent currently playing a line. There's nothing to interrupt.
			if (!isTracked)
			{
				return;
			}
			// we are playing a line but interruption is already in progress
			// we don't want to double interrupt as weird things can happen
			if (m_interruptToken.CanInterrupt)
			{
				requestInterrupt?.Invoke();
			}
		}

		/// <inheritdoc />
		/// <remarks>
		/// Stops any audio if there is still any playing.
		/// </remarks>
		public override void DialogueComplete()
		{
			// just in case we are still playing audio we want it to stop
			Stop();
		}

		#endregion

		#region Control Methods

		private bool IsAnyPlaying() => m_activeAudioSources.Any(x => x.isPlaying);

		private void Play(LocalizedLine dialogueLine, AudioClip voiceOverClip)
		{
			// Look for speaker linked to this view
			if (m_map.TryGetValue(dialogueLine.CharacterName, out var localSpeaker))
			{
				Play(localSpeaker.audioSource, voiceOverClip);
			}
			// Fallback to registered speakers in DialogueManager
			else if (DialogueManager.CastInstance.TryGetDialogueSpeakersByCharacterName(dialogueLine.CharacterName, out var speakers))
			{
				foreach (var speaker in speakers)
				{
					Play(speaker.audioSource, voiceOverClip);
				}
			}
			// Fallback to 2D speaker
			else
			{
				Play(m_audioSource, voiceOverClip);
			}
		}

		private void Play(AudioSource audioSource, AudioClip voiceOverClip)
		{
			m_activeAudioSources.Add(audioSource);
			audioSource.PlayOneShot(voiceOverClip);
		}

		private void Stop()
		{
			foreach (var audioSource in m_activeAudioSources)
			{
				audioSource.Stop();
			}
			m_activeAudioSources.Clear();
		}

		#endregion

		#region Speaker Methods

		public void AddSpeaker(DialogueSpeaker speaker)
		{
			if (m_speakers.Contains(speaker))
				return;

			m_map.Add(speaker.speakerType.characterName, speaker);
			m_speakers.Add(speaker);
		}

		public void RemoveSpeaker(DialogueSpeaker speaker)
		{
			if (!m_speakers.Contains(speaker))
				return;

			m_speakers.Remove(speaker);
			m_map.Remove(speaker.speakerType.characterName);
		}

		#endregion
	}
}
