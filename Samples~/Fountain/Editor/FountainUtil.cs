using PageOfBob.NFountain.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	public static class FountainUtil
    {
		#region Fields

		private const string SCENE_TAG = "scene";
        private const string ACTION_TAG = "action";

		#endregion

		#region Path Methods

		private static string GetYarnArtifactPath(string prefix, string extension)
		{
			string path = Path.Combine(Application.dataPath, $"../YarnArtifacts/{prefix}_{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm")}{extension}");
			path = path.Replace("\\", "/");

			Directory.CreateDirectory(Directory.GetParent(path).FullName);
			return path;
		}

		private static string ConvertToMarkdown(string text)
		{
			text = text.Replace("_", @"\_");
			return text;
		}

		private static void OpenInExplorer(string path)
		{
			Process.Start(Directory.GetParent(path).FullName);
		}

		#endregion

		#region Fountain Methods

		[MenuItem("Assets/Yarn Spinner/Export to.../Fountain", true)]
		private static bool ValidateExportToFountain()
		{
			return Selection.objects.All(x => x is YarnProject)
				|| Selection.objects.All(x => x is TextAsset);
		}

		[MenuItem("Assets/Yarn Spinner/Export to.../Fountain")]
		private static string ExportSelectedToFountain()
        {
			string path = null;
			if (Selection.objects[0] is YarnProject)
			{
				path = ExportToFountain(Selection.objects.Cast<YarnProject>(), true);
			}

			return path;
        }

		public static string ExportToFountain(YarnProject project) => ExportToFountain(new[] { project });

		private static string ExportToFountain(IEnumerable<YarnProject> projects, bool showInExplorer = false)
		{
			string path = GetYarnArtifactPath("Script", ".fountain");
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine($"Title: {Application.productName.ToUpper()}");
				writer.WriteLine($"Author: {Application.companyName}");
				writer.WriteLine("\n===\n");

				// Need dialogue to parse line attributes
				Yarn.Dialogue dialogue = null;
				string lastFile = string.Empty;
				string lastNode = string.Empty;

				foreach (var project in projects)
				{
					var importer = AssetUtil.LoadImporter<YarnProjectImporter>(project);
					var entries = importer.GenerateStringsTable();

					// Project doesn't have lines, skip
					if (!entries.Any())
						continue;

					dialogue = YarnEditorUtil.GetDialogue(project);
					lastFile = lastNode = string.Empty;

					foreach (var entry in entries)
					{
						// Processing new file
						if (!string.Equals(entry.File, lastFile))
						{
							if (!string.IsNullOrWhiteSpace(lastFile))
							{
								CloseScript(writer, lastFile);
							}

							OpenScript(writer, dialogue, entry);
							lastFile = entry.File;
						}

						// Processing new node
						if (!string.Equals(entry.Node, lastNode))
						{
							if (!string.IsNullOrWhiteSpace(lastNode))
							{
								CloseNode(writer, lastNode);
							}

							OpenNode(writer, dialogue, entry);
							lastNode = entry.Node;
						}

						string speaker = string.Empty, text = entry.Text;
						var match = Regex.Match(entry.Text, @"^(?<speaker>\w*?): ?(?<text>.*)$");
						if (match.Success)
						{
							speaker = match.Groups["speaker"].Value;
							text = match.Groups["text"].Value;
						}

						// Strip attibutes from text
						var result = dialogue.ParseMarkup(text);
						text = result.Text;

						// Speaker exists, use CHARACTER-DIALOGUE format
						if (!string.IsNullOrWhiteSpace(speaker))
						{
							writer.WriteLine(speaker.ToUpper());

							// Write parenthetical, if exists
							string parenthetical = GetParenthetical(result);
							if (!string.IsNullOrWhiteSpace(parenthetical))
							{
								writer.WriteLine($"({parenthetical})");
							}
							writer.WriteLine(text + "\n");
						}
						// Use ACTION format
						else if (!string.IsNullOrWhiteSpace(text))
						{
							writer.WriteLine(text + "\n");
						}
					}
				}

				CloseNode(writer, lastNode);
				CloseScript(writer, lastFile);
			}

			if (showInExplorer)
			{
				OpenInExplorer(path);
			}
			return path;
		}

		private static void OpenScript(StreamWriter writer, Yarn.Dialogue dialogue, StringTableEntry entry)
		{
			writer.WriteLine($">**{ConvertToMarkdown(Path.GetFileNameWithoutExtension(entry.File))}**<\n");
		}

		private static void CloseScript(StreamWriter writer, string file)
		{
			writer.WriteLine($">**End of {ConvertToMarkdown(Path.GetFileNameWithoutExtension(file))}**<\n");
		}

		private static void OpenNode(StreamWriter writer, Yarn.Dialogue dialogue, StringTableEntry entry)
		{
			IEnumerable<string> nodeTags = dialogue.GetTagsForNode(entry.Node);

			// Add scene header
			AttemptWriteLine(entry, nodeTags, SCENE_TAG, (scene) =>
			{
				writer.WriteLine($".{scene.ToUpper()}\n");
			});

			// Add action from node
			AttemptWriteLine(entry, nodeTags, ACTION_TAG, (action) =>
			{
				writer.WriteLine($"{action}\n");
			});
		}

		private static void CloseNode(StreamWriter writer, string node)
		{ }

		private static string GetParenthetical(MarkupParseResult parseResult)
		{
			foreach (var attr in parseResult.Attributes)
			{
				if (!string.Equals(attr.Name, "parenthetical", StringComparison.OrdinalIgnoreCase))
					continue;

				return attr.Properties.Values.ElementAt(0).StringValue;
			}
			return null;
		}

		//private static string ExportToFountain(IEnumerable<TextAsset> scripts)
		//{

		//}

        private static void AttemptWriteLine(StringTableEntry entry, IEnumerable<string> tags, string key, Action<string> lineWriter)
        {
            if (TryGetTag(tags, key, out string value))
            {
                lineWriter.Invoke(value);
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

		#endregion

		#region PDF Methods

		[MenuItem("Assets/Yarn Spinner/Export to.../Screenplay", true)]
		private static bool ValidateExportToPDF() => ValidateExportToFountain();

		[MenuItem("Assets/Yarn Spinner/Export to.../Screenplay")]
		private static void ExportSelectedToPDF()
		{
			string path = null;
			if (Selection.objects[0] is YarnProject)
			{
				path = ExportToPDF(Selection.objects.Cast<YarnProject>(), true);
			}
		}

		public static string ExportToPDF(YarnProject project) => ExportToPDF(new[] { project });

		private static string ExportToPDF(IEnumerable<YarnProject> projects, bool showInExplorer = false)
		{
			string fountainPath = ExportToFountain(projects);
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
			if (showInExplorer)
			{
				OpenInExplorer(path);
			}
			return path;
		}

		#endregion
	}
}