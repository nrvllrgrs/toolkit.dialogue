using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Yarn;
using Yarn.Unity;
using Yarn.Unity.Editor;
using PageOfBob.NFountain.Plugins;

namespace ToolkitEditor.Dialogue
{
	public static class YarnFileUtility
    {
		#region Fields

		private const string SCENE_TAG = "scene";
        private const string ACTION_TAG = "action";

		#endregion

		#region Fountain Methods

		//[MenuItem("Assets/Yarn Spinner/Export to Fountain...")]
        private static void ExportSelectedToFountain()
        {
            foreach (var project in Selection.objects.Cast<YarnProject>())
            {
                ExportToFountain(project);
			}
        }

        public static string ExportToFountain(YarnProject project)
        {
			// Create dialogue using project program to get node tags
			var dialogue = new Yarn.Dialogue(new EmptyVariableStorage());
			dialogue.SetProgram(project.Program);

			string assetPath = AssetDatabase.GetAssetPath(project);
			string path = Path.Combine(Application.dataPath, Path.GetDirectoryName(assetPath).Substring("Assets/".Length), $"{project.name}.fountain");

			using (StreamWriter writer = new StreamWriter(path))
			{
				var importer = AssetImporter.GetAtPath(assetPath) as YarnProjectImporter;
				var entries = importer.GenerateStringsTable();

				writer.WriteLine($"Title: {Application.productName.ToUpper()}\n");
				writer.WriteLine("====\n");

				HashSet<string> processedScenes = new();
				HashSet<string> processedActions = new();

				foreach (var entry in entries)
				{
					IEnumerable<string> nodeTags = dialogue.GetTagsForNode(entry.Node);
					IEnumerable<string> lineTags = project.lineMetadata.GetMetadata(entry.ID);

					// Add scene header
					AttemptWriteLine(processedScenes, entry, nodeTags, SCENE_TAG, (scene) =>
					{
						writer.WriteLine($".{scene.ToUpper()}\n");
					});

					// Add action from node
					AttemptWriteLine(processedActions, entry, nodeTags, ACTION_TAG, (action) =>
					{
						writer.WriteLine($"{action}\n");
					});

					// Add action from line metadata
					AttemptWriteLine(null, entry, lineTags, ACTION_TAG, (action) =>
					{
						writer.WriteLine($"{action}\n");
					});

					string speaker = string.Empty, text = string.Empty;
					var match = Regex.Match(entry.Text, @"^(?<speaker>\w*?): ?(?<text>.*)$");
					if (match.Success)
					{
						speaker = match.Groups["speaker"].Value;
						text = match.Groups["text"].Value;
					}

					if (string.IsNullOrWhiteSpace(speaker) || string.IsNullOrWhiteSpace(text))
						continue;

					writer.WriteLine(speaker.ToUpper());
					writer.WriteLine(text + "\n");
				}
			}

			AssetDatabase.Refresh();
			return path;
		}

        private static void AttemptWriteLine(HashSet<string> set, StringTableEntry entry, IEnumerable<string> tags, string key, Action<string> lineWriter)
        {
            if ((set == null || !set.Contains(entry.Node)) && TryGetTag(tags, key, out string value))
            {
                lineWriter.Invoke(value);
                set?.Add(entry.Node);
            }
        }

        private static bool TryGetTag(IEnumerable<string> tags, string key, out string value)
        {
			if (tags == null || !tags.Any())
			{
				value = default;
				return false;
			}

			value = tags.FirstOrDefault(x => x.StartsWith($"{key}:"));
			if (string.IsNullOrWhiteSpace(value))
                return false;

			value = value.Substring($"{key}:".Length);
            value = value.SnakeCaseToString();
            return true;
		}

		[MenuItem("Assets/Yarn Spinner/Export to Fountain...", true)]
		private static bool ValidateExportToFountain()
		{
			if (Selection.objects.Any(x => x is not YarnProject))
				return false;

			return true;
		}

		#endregion

		#region PDF Methods

		[MenuItem("Assets/Yarn Spinner/Export to PDF...")]
		private static void ExportSelectedToPDF()
		{
			foreach (var project in Selection.objects.Cast<YarnProject>())
			{
				ExportToPDF(project);
			}
		}

		public static string ExportToPDF(YarnProject project)
		{
			string fountainPath = ExportToFountain(project);
			string path = Path.ChangeExtension(fountainPath, ".pdf");

			using (StreamReader reader = new StreamReader(fountainPath))
			{
				using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
				{
					var writer = new PdfWriter();
					writer.Transform(new DefaultParserModule().Transform(reader), stream);
				}
			}

			File.Delete(fountainPath);

			AssetDatabase.Refresh();
			return path;
		}

		[MenuItem("Assets/Yarn Spinner/Export to PDF...", true)]
		private static bool ValidateExportToPDF() => ValidateExportToFountain();

		#endregion

		#region Structures

		private class EmptyVariableStorage : IVariableStorage
		{
			public void Clear()
			{ }

			public void SetValue(string variableName, string stringValue)
			{ }

			public void SetValue(string variableName, float floatValue)
			{ }

			public void SetValue(string variableName, bool boolValue)
			{ }

            public bool TryGetValue<T>(string variableName, out T result)
            {
                result = default(T);
                return false;
            }
		}

		#endregion
	}
}