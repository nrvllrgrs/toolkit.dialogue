using PageOfBob.NFountain.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToolkitEngine.Dialogue;
using UnityEditor;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	public static class FountainUtil
    {
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

		[MenuItem("Assets/Yarn Spinner/Export to.../Fountain", priority = 110)]
		private static void ExportSelectedToFountain()
        {
			string path = null;
			if (Selection.objects[0] is YarnProject)
			{
				path = ExportToFountain(Selection.objects.Cast<YarnProject>());
			}
			else if (Selection.objects[0] is TextAsset)
			{
				path = ExportToFountain(Selection.objects.Cast<TextAsset>());
			}

			if (!string.IsNullOrWhiteSpace(path))
			{
				OpenInExplorer(path);
			}
        }

		public static string ExportToFountain(YarnProject project) => ExportToFountain(new[] { project });

		private static string ExportToFountain(
			IEnumerable<YarnProject> projects,
			Func<StringTableEntry, bool> predicate = null)
		{
			var speakerNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var speakerType in YarnEditorUtil.GetDialogueSpeakerTypes())
			{
				speakerNameMap.Add(speakerType.name, YarnEditorUtil.GetLocalizedDisplayName(speakerType));
			}

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

					// Sort entries by order tag
					entries = YarnEditorUtil.Sort(project, entries);

					dialogue = YarnEditorUtil.GetDialogue(project);
					lastFile = lastNode = string.Empty;

					foreach (var entry in entries)
					{
						// Does not match existing predicate criteria, skip
						if (predicate != null && !predicate.Invoke(entry))
							continue;

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
							if (speakerNameMap.TryGetValue(speaker, out var displayName))
							{
								speaker = displayName;
							}

							text = match.Groups["text"].Value;
						}

						// Strip attibutes from text
						var result = dialogue.ParseMarkup(text);
						text = result.Text;

						if (TryGetChoice(entry, out int number, out string choice))
						{
							writer.WriteLine($">CHOICE {number}: {choice}\n");
						}

						// Speaker exists, use CHARACTER-DIALOGUE format
						if (!string.IsNullOrWhiteSpace(speaker))
						{
							writer.WriteLine(speaker.ToUpper());

							// Write PARENTHETICAL, if exists
							if (TryGetParenthetical(result, out string parenthetical))
							{
								writer.WriteLine($"({parenthetical})");
							}
							writer.WriteLine(text + "\n");
						}
						else
						{
							// Use ACTION format
							if (!string.IsNullOrWhiteSpace(text))
							{
								writer.WriteLine($"{text}\n");
							}
							else if (TryGetAction(result, out string action))
							{
								writer.WriteLine($"{action}\n");
							}
							// Use SCENE HEADING format
							else if (TryGetSceneHeading(result, out string scene))
							{
								writer.WriteLine($".{scene}\n");
							}
							else if (TryGetAnchor(result, out string anchor))
							{
								writer.WriteLine($">{anchor}\n");
							}
						}

						if (TryGetGoTo(entry, out string goTo))
						{
							writer.WriteLine($">GOTO: {goTo.Replace("_", " ")}\n");
						}
					}
				}

				CloseNode(writer, lastNode);
				CloseScript(writer, lastFile);
			}

			return path;
		}

		public static string ExportToFountain(TextAsset script) => ExportToFountain(new[] { script });

		private static string ExportToFountain(IEnumerable<TextAsset> scripts)
		{
			var projects = scripts.Select(x => YarnEditorUtil.FindYarnProject(x))
				.Distinct();
			var scriptNames = scripts.Select(x => x.name);

			return ExportToFountain(projects, (entry) =>
			{
				return scriptNames.Contains(Path.GetFileNameWithoutExtension(entry.File), StringComparer.OrdinalIgnoreCase);
			});
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
			writer.WriteLine($">*{ConvertToMarkdown(entry.Node)}*<\n");
		}

		private static void CloseNode(StreamWriter writer, string node)
		{
			//writer.WriteLine($">*End of {ConvertToMarkdown(node)}*<\n");
		}

		private static bool TryGetSceneHeading(MarkupParseResult result, out string value) => TryGetTag(result, "scene", out value);
		private static bool TryGetParenthetical(MarkupParseResult result, out string value) => TryGetTag(result, "parenthetical", out value);
		private static bool TryGetAction(MarkupParseResult result, out string value) => TryGetTag(result, "action", out value);
		private static bool TryGetAnchor(MarkupParseResult result, out string value) => TryGetTag(result, "anchor", out value);

		private static bool TryGetTag(MarkupParseResult result, string key, out string value)
		{
			if (result.TryGetAttributeWithName(key, out var attr))
			{
				value = attr.Properties.Values.ElementAt(0).StringValue;
				return true;
			}

			value = null;
			return false;
		}

		private static bool TryGetChoice(StringTableEntry entry, out int number, out string value)
		{
			number = 0;

			if (!YarnParserUtil.TryGetMetadataTag(entry, "choice", out value))
				return false;

			var match = Regex.Match(value, @"(?<number>\d*)\.(?<value>.*)");
			if (!match.Success)
				return false;

			number = int.Parse(match.Groups["number"].Value);
			value = match.Groups["value"].Value;
			return true;
		}

		private static bool TryGetGoTo(StringTableEntry entry, out string value) => YarnParserUtil.TryGetMetadataTag(entry, "goto", out value);

		#endregion

		#region PDF Methods

		[MenuItem("Assets/Yarn Spinner/Export to.../Screenplay", true)]
		private static bool ValidateExportToPDF() => ValidateExportToFountain();

		[MenuItem("Assets/Yarn Spinner/Export to.../Screenplay", priority = 110)]
		private static void ExportSelectedToPDF()
		{
			string path = null;
			if (Selection.objects[0] is YarnProject)
			{
				path = ExportToPDF(Selection.objects.Cast<YarnProject>());
			}
			else if (Selection.objects[0] is TextAsset)
			{
				path = ExportToPDF(Selection.objects.Cast<TextAsset>());
			}

			if (!string.IsNullOrEmpty(path))
			{
				OpenInExplorer(path);
			}
		}

		public static string ExportToPDF(YarnProject project) => ExportToPDF(new[] { project });

		private static string ExportToPDF(IEnumerable<YarnProject> projects)
		{
			return ExportToPDF(ExportToFountain(projects));
		}

		public static string ExportToPDF(TextAsset script) => ExportToPDF(new[] { script });

		private static string ExportToPDF(IEnumerable<TextAsset> scripts)
		{
			return ExportToPDF(ExportToFountain(scripts));
		}

		private static string ExportToPDF(string fountainPath)
		{
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
			return path;
		}

		#endregion
	}
}