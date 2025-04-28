using System.Collections.Generic;
using System.Reflection;
using ToolkitEngine.Dialogue;
using UnityEditor.Localization;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	public static class YarnEditorUtil
    {
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