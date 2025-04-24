using Gilzoide.EasyProjectSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToolkitEngine.Dialogue;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	[ProjectSettings(
		"ProjectSettings/DialogueSettings",
		SettingsPath = "Project/Dialogue",
		Label = "Dialogue",
		SettingsType = SettingsType.ProjectSettings)]
	public class DialogueSettings : ScriptableObject
	{
		#region Fields

		[SerializeField]
		private TTSGenerator m_generator;

		#endregion

		#region Generate Methods

		public static void Generate(YarnProject project, StringTableEntry entry)
		{
			Generate(project, new[] { entry });
		}

		public static void Generate(YarnProject project, IEnumerable<StringTableEntry> entries)
		{
			var setttings = ProjectSettings.Load<DialogueSettings>();
			setttings.m_generator?.Generate(project, entries);
		}

		public static void Generate(YarnProject project, bool onlyMismatch) => Generate(new[] { project }, onlyMismatch);

		public static void Generate(IEnumerable<YarnProject> projects, bool onlyMismatch)
		{
			Generate(projects, onlyMismatch, null);
		}

		private static void Generate(YarnProject project, bool onlyMismatch, Func<StringTableEntry, bool> predicate) => Generate(new[] { project }, onlyMismatch, predicate);

		private static void Generate(IEnumerable<YarnProject> projects, bool onlyMismatch, Func<StringTableEntry, bool> predicate)
		{
			foreach (var project in projects)
			{
				var importer = AssetUtil.LoadImporter<YarnProjectImporter>(project);
				if (importer == null)
					continue;

				var entries = importer.GenerateStringsTable();

				// Filter entries by predicate
				if (predicate != null)
				{
					entries = entries.Where(x => predicate(x));
				}

				// Filter entries by mismatch
				if (onlyMismatch)
				{
					entries = entries.Where(x => !IsSpeakerAndTextMatch(project, x));
				}

				Generate(project, entries);
			}
		}

		public static void Generate(TextAsset script, bool onlyMismatch) => Generate(new[] { script }, onlyMismatch);

		public static void Generate(IEnumerable<TextAsset> scripts, bool onlyMismatch)
		{
			foreach (var g in scripts.GroupBy(x => YarnEditorUtil.FindYarnProject(x)))
			{
				IEnumerable<string> scriptNames = g.Select(x => x.name);
				Generate(g.Key, onlyMismatch, (entry) =>
				{
					return scriptNames.Contains(Path.GetFileNameWithoutExtension(entry.File), StringComparer.OrdinalIgnoreCase);
				});
			}
		}

		public static void Generate(DialogueSpeakerType speakerType, bool onlyMismatch) => Generate(new[] { speakerType }, onlyMismatch);

		public static void Generate(IEnumerable<DialogueSpeakerType> speakerTypes, bool onlyMismatch)
		{
			var speakerNames = speakerTypes.Select(x => x.name);
			Generate(YarnEditorUtil.GetYarnProjects(), onlyMismatch, (entry) =>
			{
				return YarnParserUtil.TryGetSpeakerAndText(entry, out var speaker, out var text)
					&& speakerNames.Contains(speaker, StringComparer.OrdinalIgnoreCase);
			});
		}

		#endregion

		#region Menu Methods

		private static bool ValidateGenerateSelected()
		{
			return Selection.objects.All(x => x is YarnProject)
				|| Selection.objects.All(x => x is TextAsset)
				|| Selection.objects.All(x => x is DialogueSpeakerType);
		}

		[MenuItem("Assets/Yarn Spinner/TTS/Generate All", validate = true)]
		private static bool ValidateGenerateSelectedAll() => ValidateGenerateSelected();

		[MenuItem("Assets/Yarn Spinner/TTS/Generate All")]
		private static void GenerateSelectedAll()
		{
			if (Selection.objects[0] is YarnProject)
			{
				Generate(Selection.objects.Cast<YarnProject>(), false);
			}
			else if (Selection.objects[0] is TextAsset)
			{
				Generate(Selection.objects.Cast<TextAsset>(), false);
			}
			else if (Selection.objects[0] is DialogueSpeakerType)
			{
				Generate(Selection.objects.Cast<DialogueSpeakerType>(), false);
			}
		}

		[MenuItem("Assets/Yarn Spinner/TTS/Generate Mismatch", validate = true)]
		private static bool ValidateGenerateSelectedMismatch() => ValidateGenerateSelected();

		[MenuItem("Assets/Yarn Spinner/TTS/Generate Mismatch")]
		private static void GenerateSelectedMismatch()
		{
			if (Selection.objects[0] is YarnProject)
			{
				Generate(Selection.objects.Cast<YarnProject>(), true);
			}
			else if (Selection.objects[0] is TextAsset)
			{
				Generate(Selection.objects.Cast<TextAsset>(), true);
			}
			else if (Selection.objects[0] is DialogueSpeakerType)
			{
				Generate(Selection.objects.Cast<DialogueSpeakerType>(), true);
			}
		}

		#endregion

		#region Metadata Methods

		public static bool TryGetSpeakerAndTextTags(string path, out string speaker, out string text)
		{
			if (!File.Exists(path))
			{
				speaker = text = default;
				return false;
			}

			var tagFile = TagLib.File.Create(path);
			speaker = tagFile.Tag.FirstPerformer;
			text = tagFile.Tag.Subtitle;
			return true;
		}

		public static void SetSpeakerAndTextTags(string path, string speaker, string text)
		{
			if (!File.Exists(path))
				return;

			var tagFile = TagLib.File.Create(path);
			tagFile.Tag.Performers = new[] { speaker };
			tagFile.Tag.Subtitle = text;
			tagFile.Save();
		}

		public static bool IsSpeakerAndTextMatch(YarnProject project, StringTableEntry entry)
		{
			var clip = YarnEditorUtil.GetPreviewClip(project, entry);
			var path = FileUtil.GetAbsolutePath(clip);

			if (!TryGetSpeakerAndTextTags(path, out var speakerTag, out var textTag)
				|| !YarnParserUtil.TryGetSpeakerAndText(entry, out var speaker, out var text))
				return false;

			return string.Equals(speakerTag, speaker, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(textTag, text, StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}