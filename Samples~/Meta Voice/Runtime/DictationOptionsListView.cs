using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using Yarn.Unity;
using System.Collections;
using Meta.WitAi.Dictation;
using System.Text.RegularExpressions;

namespace ToolkitEngine.Dialogue.Voice
{
	public class DictationOptionsListView : DialogueViewBase
	{
		#region Fields

		[SerializeField]
		private CanvasGroup m_canvasGroup;

		[SerializeField]
		private OptionView m_optionViewPrefab;

		[SerializeField]
		private MarkupPalette m_palette;

		[SerializeField]
		private float m_fadeTime = 0.1f;

		[SerializeField]
		private bool m_showUnavailableOptions = false;

		[Header("Last Line Components")]

		[SerializeField]
		private TextMeshProUGUI m_lastLineText;

		[SerializeField]
		private GameObject m_lastLineContainer;

		[SerializeField]
		private TextMeshProUGUI m_lastLineCharacterNameText;

		[SerializeField]
		private GameObject m_lastLineCharacterNameContainer;

		/// <summary>
		/// A cached pool of OptionView objects so that we can reuse them 
		/// </summary>
		private List<OptionView> m_optionViews = new List<OptionView>();

		/// <summary>
		/// Map of text-to-OptionView
		/// </summary>
		private Dictionary<string, OptionView> m_optionMap = new();

		/// <summary>
		/// The method we should call when an option has been selected.
		/// </summary>
		private Action<int> m_onOptionSelected;

		/// <summary>
		/// The line we saw most recently.
		/// </summary>
		private LocalizedLine m_lastSeenLine;

		private DictationService m_dictation;

		/// <summary>
		/// Indicates whether voice key was successfully found
		/// </summary>
		private bool m_success;

		#endregion

		#region Methods

		public void Awake()
		{
			if (!m_dictation)
			{
				m_dictation = FindFirstObjectByType<DictationService>();
			}
		}

		public void Start()
		{
			m_canvasGroup.alpha = 0;
			m_canvasGroup.interactable = false;
			m_canvasGroup.blocksRaycasts = false;
		}

		public void OnEnable()
		{
			Relayout();
		}

		public void Reset()
		{
			m_canvasGroup = GetComponentInParent<CanvasGroup>();
		}

		#endregion

		#region Yarn Callbacks

		public override void DialogueStarted()
		{
			m_dictation.DictationEvents.OnFullTranscription.AddListener(Dictation_Transcription);
			m_dictation.DictationEvents.OnPartialTranscription.AddListener(Dictation_Transcription);
			m_dictation.DictationEvents.OnRequestCompleted.AddListener(Dictation_StoppedListening);
		}

		public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
		{
			// Don't do anything with this line except note it and
			// immediately indicate that we're finished with it. RunOptions
			// will use it to display the text of the previous line.
			m_lastSeenLine = dialogueLine;
			onDialogueLineFinished();
		}
		public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
		{
			m_optionMap.Clear();

			// If we don't already have enough option views, create more
			while (dialogueOptions.Length > m_optionViews.Count)
			{
				var optionView = CreateNewOptionView();
				optionView.gameObject.SetActive(false);
			}

			// Set up all of the option views
			int optionViewsCreated = 0;

			for (int i = 0; i < dialogueOptions.Length; i++)
			{
				var optionView = m_optionViews[i];
				var option = dialogueOptions[i];

				if (option.IsAvailable == false && m_showUnavailableOptions == false)
				{
					// Don't show this option.
					continue;
				}

				optionView.gameObject.SetActive(true);

				optionView.palette = this.m_palette;
				optionView.Option = option;

				// Add to map AFTER option (and line) has been assigned
				string key = option.Line.TextWithoutCharacterName.Text.ToLower();
				key = Regex.Replace(key, @"[^a-z ]", string.Empty);
				m_optionMap.Add(key, optionView);

				// The first available option is selected by default
				if (optionViewsCreated == 0)
				{
					optionView.Select();
				}

				optionViewsCreated += 1;
			}

			// Update the last line, if one is configured
			if (m_lastLineContainer != null)
			{
				if (m_lastSeenLine != null)
				{
					// if we have a last line character name container
					// and the last line has a character then we show the nameplate
					// otherwise we turn off the nameplate
					var line = m_lastSeenLine.Text;
					if (m_lastLineCharacterNameContainer != null)
					{
						if (string.IsNullOrWhiteSpace(m_lastSeenLine.CharacterName))
						{
							m_lastLineCharacterNameContainer.SetActive(false);
						}
						else
						{
							line = m_lastSeenLine.TextWithoutCharacterName;
							m_lastLineCharacterNameContainer.SetActive(true);
							m_lastLineCharacterNameText.text = m_lastSeenLine.CharacterName;
						}
					}

					if (m_palette != null)
					{
						m_lastLineText.text = LineView.PaletteMarkedUpText(line, m_palette);
					}
					else
					{
						m_lastLineText.text = line.Text;
					}

					m_lastLineContainer.SetActive(true);
				}
				else
				{
					m_lastLineContainer.SetActive(false);
				}
			}

			// Note the delegate to call when an option is selected
			m_onOptionSelected = onOptionSelected;

			// sometimes (not always) the TMP layout in conjunction with the
			// content size fitters doesn't update the rect transform
			// until the next frame, and you get a weird pop as it resizes
			// just forcing this to happen now instead of then
			Relayout();

			// Fade it all in
			StartCoroutine(Effects.FadeAlpha(m_canvasGroup, 0, 1, m_fadeTime));

			// Start listening to user, selecting option with voice
			m_success = false;
			m_dictation.Activate();

			/// <summary>
			/// Creates and configures a new <see cref="OptionView"/>, and adds
			/// it to <see cref="m_optionViews"/>.
			/// </summary>
			OptionView CreateNewOptionView()
			{
				var optionView = Instantiate(m_optionViewPrefab);
				optionView.transform.SetParent(transform, false);
				optionView.transform.SetAsLastSibling();

				optionView.OnOptionSelected = OptionViewWasSelected;
				m_optionViews.Add(optionView);

				return optionView;
			}

			/// <summary>
			/// Called by <see cref="OptionView"/> objects.
			/// </summary>
			void OptionViewWasSelected(DialogueOption option)
			{
				StartCoroutine(OptionViewWasSelectedInternal(option));

				IEnumerator OptionViewWasSelectedInternal(DialogueOption selectedOption)
				{
					yield return StartCoroutine(FadeAndDisableOptionViews(m_canvasGroup, 1, 0, m_fadeTime));
					m_onOptionSelected(selectedOption.DialogueOptionID);
				}
			}
		}

		/// <inheritdoc />
		/// <remarks>
		/// If options are still shown dismisses them.
		/// </remarks>
		public override void DialogueComplete()
		{
			m_dictation.DictationEvents.OnFullTranscription.RemoveListener(Dictation_Transcription);
			m_dictation.DictationEvents.OnPartialTranscription.RemoveListener(Dictation_Transcription);
			m_dictation.DictationEvents.OnRequestCompleted.RemoveListener(Dictation_StoppedListening);

			// do we still have any options being shown?
			if (m_canvasGroup.alpha > 0)
			{
				StopAllCoroutines();
				m_lastSeenLine = null;
				m_onOptionSelected = null;
				m_canvasGroup.interactable = false;
				m_canvasGroup.blocksRaycasts = false;

				StartCoroutine(FadeAndDisableOptionViews(m_canvasGroup, m_canvasGroup.alpha, 0, m_fadeTime));
			}
		}

		/// <summary>
		/// Fades canvas and then disables all option views.
		/// </summary>
		private IEnumerator FadeAndDisableOptionViews(CanvasGroup canvasGroup, float from, float to, float fadeTime)
		{
			yield return Effects.FadeAlpha(canvasGroup, from, to, fadeTime);

			// Hide all existing option views
			foreach (var optionView in m_optionViews)
			{
				optionView.gameObject.SetActive(false);
			}
		}

		private void Relayout()
		{
			// Force re-layout
			var layouts = GetComponentsInChildren<UnityEngine.UI.LayoutGroup>();

			// Perform the first pass of re-layout. This will update the inner horizontal group's sizing, based on the text size.
			foreach (var layout in layouts)
			{
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
			}

			// Perform the second pass of re-layout. This will update the outer vertical group's positioning of the individual elements.
			foreach (var layout in layouts)
			{
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
			}
		}

		#endregion

		#region Dictation Callbacks

		private void Dictation_Transcription(string text)
		{
			text = text.ToLower();
			if (m_optionMap.TryGetValue(text, out var optionView))
			{
				optionView.InvokeOptionSelected();

				// Stop listening to user
				m_success = true;
				m_dictation.Deactivate();
			}
		}

		private void Dictation_StoppedListening()
		{
			if (!m_success)
			{
				m_dictation.Activate();
			}
		}

		#endregion

	}
}