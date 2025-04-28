using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToolkitEngine.Dialogue;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	public abstract class TTSGenerator : ScriptableObject
	{
		#region Fields

		[SerializeField]
		protected bool m_importAssets;

		[SerializeField]
		protected DefaultAsset m_directory;

		#endregion

		#region Events

		public static Action GenerationCompleted;

		#endregion

		#region Methods

		public void Generate(YarnProject project, IEnumerable<StringTableEntry> entries)
		{
			EditorCoroutineUtility.StartCoroutine(AsyncGenerate(project, entries), this);
		}

		protected abstract IEnumerator AsyncGenerate(YarnProject project, IEnumerable<StringTableEntry> entries);

		#endregion
	}

	public abstract class TTSGenerator<T> : TTSGenerator
		where T : TTSVoice
    {
		#region Methods

		protected override IEnumerator AsyncGenerate(
			YarnProject project,
			IEnumerable<StringTableEntry> entries)
		{
			Dictionary<string, DialogueSpeakerType> speakerTypeMap = new Dictionary<string, DialogueSpeakerType>(StringComparer.OrdinalIgnoreCase);
			foreach (var speakerType in YarnEditorUtil.GetDialogueSpeakerTypes())
			{
				speakerTypeMap.Add(speakerType.name, speakerType);
			}

			// Create dialogue to parse markup
			var dialogue = YarnEditorUtil.GetDialogue(project);

			int i = 0;
			float total = entries.Count();
			foreach (var entry in entries)
			{
				ProgressBarUtil.DisplayProgressBar(
					$"Generating {typeof(T).Name}...",
					$"{entry.Text}...{i + 1}/{(int)total}...",
					i++ / total);

				if (YarnParserUtil.TryGetSpeakerAndText(entry, out var speaker, out var text)
					&& speakerTypeMap.TryGetValue(speaker, out var speakerType)
					&& speakerType.ttsVoice is T ttSVoice)
				{
					MarkupParseResult? result = null;
					try
					{
						// Strip attributes for TTS generation
						result = dialogue.ParseMarkup(text);
					}
					catch (Exception e)
					{
						Debug.LogError(e.Message);
					}

					if (result.HasValue)
					{
						yield return AsyncGenerate(project, dialogue, entry, result.Value.Text, ttSVoice, (path) =>
						{
							// Use Voice asset name because separate speakers may have different post-processing
							// ...but could reference the same asset
							// Want to include attributes in metadata
							DialogueSettings.SetSpeakerAndTextTags(path, ttSVoice.voiceName, text);
							Debug.Log($"Generated {path.Replace("\\", "/")}");

							// Reimport AudioClip
							var importer = AssetImporter.GetAtPath(FileUtil.GetRelativePath(path)) as AudioImporter;
							importer?.SaveAndReimport();
						});
					}
				}
			}

			yield return EditorCoroutineUtility.StartCoroutine(AsyncFinishGenerate(project), this);

			ProgressBarUtil.ClearProgressBar();

			// Notify subscribers (ex. Yarn Viewer) that generation finished)
			GenerationCompleted?.Invoke();
		}

		protected abstract IEnumerator AsyncGenerate(YarnProject project, Yarn.Dialogue dialogue, StringTableEntry entry, string text, T ttsVoice, Action<string> callback);

		protected virtual IEnumerator AsyncFinishGenerate(YarnProject project)
		{
			if (m_importAssets)
			{
				var importer = AssetUtil.LoadImporter<YarnProjectImporter>(project);
				importer?.SaveAndReimport();
			}
			yield return null;
		}

		#endregion
	}
}