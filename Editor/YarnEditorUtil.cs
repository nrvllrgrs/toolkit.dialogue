using System;
using System.Collections.Generic;
using System.Reflection;
using ToolkitEngine.Dialogue;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Editor;
using System.Linq;
using System.Text.RegularExpressions;



#if USE_UNITY_LOCALIZATION
using UnityEditor.Localization;
using UnityEngine.Localization;
#endif

namespace ToolkitEditor.Dialogue
{
	public static class YarnEditorUtil
    {
		#region Fields

		private const string ORDER_TAG = "order:";

		#endregion

		public static IEnumerable<YarnProject> GetYarnProjects() => AssetUtil.GetAssetsOfType<YarnProject>();
        public static IEnumerable<DialogueSpeakerType> GetDialogueSpeakerTypes() => AssetUtil.GetAssetsOfType<DialogueSpeakerType>();

        public static Yarn.Dialogue GetDialogue(YarnProject project)
        {
            var dialogue = new Yarn.Dialogue(new EmptyVariableStorage());
            dialogue.SetProgram(project.Program);

            return dialogue;
		}

        public static YarnProject FindYarnProject(TextAsset script)
        {
            foreach (var project in GetYarnProjects())
            {
                var importer = AssetUtil.LoadImporter<YarnProjectImporter>(project);
                if (importer == null)
                    continue;

                if (importer.ImportData.yarnFiles.Contains(script))
                    return project;
            }

            return null;
        }

		public static IEnumerable<StringTableEntry> Sort(YarnProject project, IEnumerable<StringTableEntry> entries)
		{
			int act = 0, scene = 0, beat = 0;
			return from entry in entries
				   let hasOrder = TryGetOrder(project, entry, out act, out scene, out beat)
				   orderby hasOrder descending, act ascending, scene ascending, beat ascending
				   select entry;
		}

		public static bool TryGetOrder(YarnProject project, StringTableEntry entry, out int act, out int scene, out int beat)
		{
			act = scene = beat = 0;
			var nodeTags = GetDialogue(project)?.GetTagsForNode(entry.Node);
			if (nodeTags == null)
				return false;

			var orderTag = nodeTags.FirstOrDefault(x => x.StartsWith(ORDER_TAG, StringComparison.OrdinalIgnoreCase));
			if (orderTag == null)
				return false;

			string[] s = orderTag.Substring(ORDER_TAG.Length).Split('.');
			if (s.Length != 3)
				return false;

			return int.TryParse(s[0], out act)
				&& int.TryParse(s[1], out scene)
				&& int.TryParse(s[2], out beat);
		}

		public static void AddLineTagsToFilesInYarnProject(YarnProject project)
		{
			if (project == null)
				return;

			AddLineTagsToFilesInYarnProject(AssetUtil.LoadImporter<YarnProjectImporter>(project));
		}

		public static void AddLineTagsToFilesInYarnProject(YarnProjectImporter importer)
		{
			if (importer == null)
				return;

			var methodInfo = typeof(YarnProjectEditor).Assembly.GetType("Yarn.Unity.Editor.YarnProjectUtility")
					.GetMethod("AddLineTagsToFilesInYarnProject", BindingFlags.Static | BindingFlags.NonPublic);
			if (methodInfo != null)
			{
				methodInfo.Invoke(null, new[] { importer });
			}
		}

		public static string GetLocalizedDisplayName(DialogueSpeakerType speakerType)
		{
			if (speakerType == null)
				return speakerType.name;

#if USE_UNITY_LOCALIZATION

			var fieldInfo = typeof(DialogueSpeakerType).GetField("m_displayName", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo != null)
			{
				var localizedString = fieldInfo.GetValue(speakerType) as LocalizedString;
				if (localizedString != null)
				{
					return LocalizedStringEditorExt.GetLocalizedStringImmediate(localizedString);
				}
			}
#endif

			return speakerType.displayName;
		}

		#region Audio Methods

		public static AudioClip GetPreviewClip(YarnProject project, StringTableEntry entry)
		{
			string localeCode = string.Empty;
			switch (project.localizationType)
			{
				case LocalizationType.YarnInternal:
					localeCode = project.baseLocalization?.LocaleCode;
					break;

#if USE_UNITY_LOCALIZATION
				case LocalizationType.Unity:
					localeCode = LocalizationEditorSettings.ActiveLocalizationSettings.GetSelectedLocale()?.Identifier.Code;
					break;
#endif
			}
			return GetPreviewClip(project, entry, localeCode);
		}

		public static AudioClip GetPreviewClip(YarnProject project, StringTableEntry entry, string localeCode)
		{
			AudioClip clip = null;
			switch (project.localizationType)
			{
				case LocalizationType.YarnInternal:
					clip = project.GetLocalization(localeCode)?.GetLocalizedObject<AudioClip>(entry.ID);
					break;

#if USE_UNITY_LOCALIZATION
				case LocalizationType.Unity:
					var activeLocalization = LocalizationEditorSettings.ActiveLocalizationSettings;


					//var record = s_assetTableCollections.Select(x => x.GetTable(localeCode) as AssetTable)
					//	.Select(x => x.GetEntry(value.entry.ID))
					//	.Where(x => x != null)
					//	.FirstOrDefault();

					//if (record != null)
					//{
					//	clip = activeLocalization.GetAssetDatabase()?.GetLocalizedAsset<AudioClip>(record.Table.TableCollectionName, record.KeyId);
					//}
					break;
#endif
			}
			return clip;
		}

		#endregion
	}
}