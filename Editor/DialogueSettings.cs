using Gilzoide.EasyProjectSettings;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue
{
	[ProjectSettings("ProjectSettings/DialogueSettings", SettingsPath = "Project/Dialogue", Label = "Dialogue", SettingsType = SettingsType.ProjectSettings)]
	public class DialogueSettings : ScriptableObject
	{
		#region Fields

		[SerializeField]
		private TTSGenerator m_generator;

		#endregion

		#region Methods

		public void Generate(YarnProject project, StringTableEntry entry)
		{
			Generate(project, new[] { entry });
		}

		public void Generate(YarnProject project, IEnumerable<StringTableEntry> entries)
		{
			m_generator?.Generate(project, entries);
		}

		public static void GetSpeakerAndTextTags(string path, out string speaker, out string text)
		{
			var tagFile = TagLib.File.Create(path);
			speaker = tagFile.Tag.FirstPerformer;
			text = tagFile.Tag.Subtitle;
		}

		public static void SetSpeakerAndTextTags(string path, string speaker, string text)
		{
			var tagFile = TagLib.File.Create(path);
			tagFile.Tag.Performers = new[] { speaker };
			tagFile.Tag.Subtitle = text;
			tagFile.Save();
		}

		#endregion
	}
}