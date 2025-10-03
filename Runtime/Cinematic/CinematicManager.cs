using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class CinematicManager : ConfigurableSubsystem<CinematicManager, CinematicManagerConfig>
    {
		#region Fields

		private bool m_skippable = false;
		private string m_skipDestination = null;
		private bool m_waiting = false;

		private float m_remainingTime = 0f;
		private float m_timeout = 0f;

		private DialogueRunnerControl m_cinematicControl;
		private int m_animateStateHash;

		#endregion

		#region Events

		public event EventHandler<bool> SkippableChanged;

		#endregion

		#region Properties

		public string skipDestination
		{
			get => m_skipDestination;
			private set
			{
				// No change, skip
				if (m_skipDestination == value)
					return;

				m_skipDestination = value;
				skippable = !string.IsNullOrWhiteSpace(m_skipDestination);
			}
		}

		public bool skippable
		{
			get => m_skippable;
			private set
			{
				// No change, skip
				if (m_skippable == value)
					return;

				bool wasSkippable = skippable;
				m_skippable = value;

				if (wasSkippable != skippable)
				{
					SkippableChanged?.Invoke(this, !wasSkippable);
				}
			}
		}

		public float remainingTime => m_remainingTime;
		public float normalizedRemainingTime => m_remainingTime / m_timeout;

		#endregion

		#region Methods

		protected override void Initialize()
		{
			DialogueManager.CastInstance.DialogueStarted += DialogueManager_DialogueStarted;
			m_animateStateHash = Animator.StringToHash(Config.animateStateName);
		}

		protected override void Terminate()
		{
			if (DialogueManager.Exists)
			{
				DialogueManager.CastInstance.DialogueStarted -= DialogueManager_DialogueStarted;
			}
		}

		public void Skip()
		{
			if (!skippable || m_cinematicControl == null)
				return;

			m_cinematicControl.Stop(true);
			if (!string.IsNullOrWhiteSpace(m_skipDestination))
			{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				m_cinematicControl.Play(skipDestination);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				skipDestination = null;
			}
		}

		public void Continue()
		{
			m_waiting = false;
		}

		#endregion

		#region Callbacks

		private void DialogueManager_DialogueStarted(object sender, DialogueEventArgs e)
		{
			if (Config == null)
				return;

			if (e.control.dialogueType != Config.dialogueType)
				return;

			// Store DialogueRunnerControl associated with "cinematic" DialogueType
			m_cinematicControl = e.control;
		}

		#endregion

		#region Skip

		[YarnCommand("skipAndEnd")]
		public static void SetupSkipAndEnd()
		{
			CleanupSkip();
			CastInstance.skippable = true;

			DialogueManager.CastInstance.DialogueCompleted += Skip_DialogueCompleted;
		}

		[YarnCommand("skip")]
		public static void SetupSkip(string destinationNode)
		{
			CleanupSkip();

			// Store destination of skipping
			CastInstance.skipDestination = destinationNode;

			DialogueManager.CastInstance.NodeStarted += Skip_NodeStarted;
			DialogueManager.CastInstance.DialogueCompleted += Skip_DialogueCompleted;
		}

		private static void Skip_NodeStarted(object sender, DialogueEventArgs e)
		{
			if (!Equals(e.control, CastInstance.m_cinematicControl)
				|| !Equals(e.nodeName, CastInstance.skipDestination))
				return;

			CleanupSkip();
		}

		private static void Skip_DialogueCompleted(object sender, DialogueEventArgs e)
		{
			// If Cinematic Dialogue ends before reaching skip node, cleanup
			if (!Equals(e.control, CastInstance.m_cinematicControl))
				return;

			CleanupSkip();
		}

		private static void CleanupSkip()
		{
			// Clear destination...cannot skip anymore
			CastInstance.skipDestination = null;

			// Stop watching events
			DialogueManager.CastInstance.NodeStarted -= Skip_NodeStarted;
			DialogueManager.CastInstance.DialogueCompleted -= Skip_DialogueCompleted;
		}

		#endregion

		#region Wait For Continue

		[YarnCommand("waitForContinue")]
		public static IEnumerator WaitForContinue()
		{
			CastInstance.m_waiting = true;
			yield return new WaitWhile(() => CastInstance.m_waiting);
		}

		[YarnCommand("waitForContinueWithTimeout")]
		public static IEnumerator WaitForContinueWithTimeout(float timeout, string variableName)
		{
			CastInstance.m_waiting = true;
			CastInstance.m_remainingTime = CastInstance.m_timeout = timeout;

			while (CastInstance.m_remainingTime > 0f)
			{
				yield return null;

				if (!CastInstance.m_waiting)
				{
					SetVariable(variableName, true);
				}
				CastInstance.m_remainingTime -= Time.deltaTime;
			}

			CastInstance.m_waiting = false;
			SetVariable(variableName, false);
		}

		private static void SetVariable(string variableName, bool value)
		{
			if (CastInstance.m_cinematicControl == null)
				return;

			var variableStorage = CastInstance.m_cinematicControl.dialogueRunner.VariableStorage;
			variableStorage.SetValue(variableName, value);
		}

		#endregion

		#region Animate

		[YarnCommand("animate")]
		public static void Animate(string characterName, string animationKey)
		{
			Animate(characterName, animationKey, CastInstance.Config.animateStateName, CastInstance.m_animateStateHash);
		}

		[YarnCommand("customAnimate")]
		public static void Animate(string characterName, string animationKey, string animStateName)
		{
			Animate(characterName, animationKey, animStateName, Animator.StringToHash(animStateName));
		}

		public static void Animate(string characterName, string animationKey, string animStateName, int animStateHash)
		{
			if (DialogueManager.CastInstance.TryGetDialogueSpeakerTypeByCharacterName(characterName, out var speakerType)
			   && DialogueManager.CastInstance.TryGetDialogueSpeakers(speakerType, out var speakers))
			{
				Animate(speakerType, speakers, animationKey, animStateName, animStateHash);
			}
		}

		public static void Animate(DialogueSpeakerType speakerType, HashSet<DialogueSpeaker> speakers, string animationKey)
		{
			Animate(speakerType, speakers, animationKey, CastInstance.Config.animateStateName, CastInstance.m_animateStateHash);
		}

		public static void Animate(DialogueSpeakerType speakerType, HashSet<DialogueSpeaker> speakers, string animationKey, string animStateName)
		{
			Animate(speakerType, speakers, animationKey, CastInstance.Config.animateStateName, Animator.StringToHash(animStateName));
		}

		public static void Animate(DialogueSpeakerType speakerType, HashSet<DialogueSpeaker> speakers, string animationKey, string animStateName, int animStateHash)
		{
			if (speakers == null)
				return;

			// Set animation for each found speaker
			foreach (var speaker in speakers)
			{
				AnimationClip clip = null;
				if ((speaker.GetComponent<AnimationSetOverride>()?.TryGetClip(animationKey, out clip) ?? false)
					|| (speakerType.animationSet?.TryGetClip(animationKey, out clip) ?? false))
				{
					var animatorStack = speaker.GetComponent<AnimatorStack>();
					if (animatorStack != null)
					{
						animatorStack.Clear();
						animatorStack.Push(clip, animStateName);
						animatorStack.animator.Play(animStateHash, 0);
					}
				}
			}
		}

		#endregion
	}
}