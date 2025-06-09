using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToolkitEngine.Dialogue;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Yarn.Unity;
using Yarn.Unity.Editor;
using Yarn.Markup;


#if USE_UNITY_LOCALIZATION
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
#endif

namespace ToolkitEditor.Dialogue
{
	public class YarnViewer : EditorWindow
	{
		#region Fields

		private static HashSet<YarnStringEntry> s_entries = new();
		private static List<YarnStringEntry> s_filteredEntries = new();

#if USE_UNITY_LOCALIZATION
		private static HashSet<StringTableCollection> s_stringTableCollections = new();
		private static HashSet<AssetTableCollection> s_assetTableCollections = new();
#endif

		private static YarnViewer s_window;
		private static ToolbarSearchField s_searchField;
		private static MultiColumnListView s_columnListView;

		private const float BUTTON_HEIGHT = 20f;
		private const float BUTTON_WIDTH = 30f;

		#endregion

		#region Methods

		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			if (HasOpenInstances<YarnViewer>())
			{
				RefreshEntries();
			}
		}

		[MenuItem("Window/Yarn Spinner/Yarn Viewer")]
		public static void ShowWindow()
		{
			RefreshEntries();

			s_window = GetWindow<YarnViewer>();
			s_window.titleContent = new GUIContent("Yarn Viewer");
		}

		private static void RefreshEntries()
		{
			s_entries.Clear();
			s_filteredEntries.Clear();

#if USE_UNITY_LOCALIZATION
			s_stringTableCollections.Clear();
			s_assetTableCollections.Clear();
#endif

			foreach (var project in YarnEditorUtil.GetYarnProjects())
			{
				var importer = AssetUtil.LoadImporter<YarnProjectImporter>(project);
				if (importer == null)
					continue;

#if USE_UNITY_LOCALIZATION
				if (importer.UseUnityLocalisationSystem)
				{
					s_stringTableCollections.Add(importer.unityLocalisationStringTableCollection);

					var guid = AssetDatabase.FindAssets($"t:AssetTableCollection {importer.unityLocalisationStringTableCollection}_VO")
						.FirstOrDefault();
					if (guid != null)
					{
						s_assetTableCollections.Add(AssetDatabase.LoadAssetAtPath<AssetTableCollection>(AssetDatabase.GUIDToAssetPath(guid)));
					}
				}
#endif
				// Want to be sure Line IDs are assigned before possibly generating TTS
				YarnEditorUtil.AddLineTagsToFilesInYarnProject(importer);

				var dialogue = YarnEditorUtil.GetDialogue(project);
				foreach (var entry in importer.GenerateStringsTable())
				{
					MarkupParseResult? result = null;
					try
					{
						// Strip attributes for TTS generation
						result = dialogue.ParseMarkup(entry.Text);
					}
					catch { }

					// Line cannot be parsed OR does not exist, skip
					if (!result.HasValue || string.IsNullOrWhiteSpace(result.Value.Text))
						continue;

					var yarnEntry = new YarnStringEntry()
					{
						project = project,
						entry = entry,
					};

					yarnEntry.speakerTextMetadataMatch = DialogueSettings.IsSpeakerAndTextMatch(project, entry);

					if (importer.UseUnityLocalisationSystem)
					{
#if USE_UNITY_LOCALIZATION
						var locale = LocalizationEditorSettings.ActiveLocalizationSettings.GetSelectedLocale();
						if (locale != null)
						{
							var record = s_stringTableCollections.Select(x => x.GetTable(locale.Identifier) as StringTable)
								.Select(x => x.GetEntry(entry.ID))
								.Where(x => x != null)
								.FirstOrDefault();
							yarnEntry.stringInTable = !string.IsNullOrWhiteSpace(record?.Value);
							yarnEntry.audioInTable = YarnEditorUtil.GetPreviewClip(yarnEntry.project, yarnEntry.entry, locale.Identifier.Code) != null;
						}
#endif
					}

					s_entries.Add(yarnEntry);
				}
			}

			Search(s_searchField?.value ?? string.Empty);
		}

		private void Awake()
		{
			TTSGenerator.GenerationCompleted += RefreshEntries;
		}

		private void OnDestroy()
		{
			TTSGenerator.GenerationCompleted -= RefreshEntries;

			AudioUtil.StopAllPreviewClips();
			s_window = null;
		}

		public void CreateGUI()
		{
			VisualElement root = rootVisualElement;

			var header = new VisualElement();
			header.style.flexDirection = FlexDirection.Row;
			header.style.flexShrink = 0;
			header.style.height = StyleKeyword.Auto;
			{
				s_searchField = new ToolbarSearchField();
				s_searchField.RegisterValueChangedCallback(Search);
				header.Add(s_searchField);

				var icon = EditorGUIUtility.IconContent("Refresh");
				var refreshButton = new Button()
				{
					iconImage = new Background()
					{
						texture = icon.image as Texture2D,
					},
					tooltip = "Refresh",
				};
				refreshButton.RegisterCallback<ClickEvent>(RefreshButtonClicked);
				header.Add(refreshButton);

				var generateAllButton = new Button()
				{
					iconImage = new Background()
					{
						texture = AssetUtil.LoadFirstAsset<Texture2D>("GenerateTTS EditorIcon"),
					},
					tooltip = "Generate All",
				};
				generateAllButton.style.minHeight = generateAllButton.style.maxHeight = BUTTON_HEIGHT;
				generateAllButton.style.minWidth = generateAllButton.style.maxWidth = BUTTON_WIDTH;
				generateAllButton.RegisterCallback<ClickEvent>(GenerateAllButtonClicked);
				header.Add(generateAllButton);
			}
			root.Add(header);

			s_columnListView = new MultiColumnListView()
			{
				itemsSource = s_filteredEntries,
				selectionType = SelectionType.Single,
				showBoundCollectionSize = true,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
			};

			AddColumn("ID", true, true, null, null, (element, index) =>
			{
				(element as Label).text = YarnParserUtil.GetID(s_filteredEntries[index].entry);
			});
			AddColumn("Speaker", true, true, null, null, (element, index) =>
			{
				YarnParserUtil.TryGetSpeakerAndText(s_filteredEntries[index].entry, out string speaker, out string text);
				(element as Label).text = speaker;
			});
			AddColumn("Text", true, true, null, null, (element, index) =>
			{
				YarnParserUtil.TryGetSpeakerAndText(s_filteredEntries[index].entry, out string speaker, out string text);
				(element as Label).text = text;
			});
			AddColumn("Metadata", true, true, null, null, (element, index) =>
			{
				(element as Label).text = YarnParserUtil.GetMetadata(s_filteredEntries[index].entry);
			});
			AddColumn("Preview", false, false, null, GetPreviewButton, (element, index) =>
			{
				var value = s_filteredEntries[index];
				var button = element as Button;
				button.userData = index;
				button.SetEnabled(YarnEditorUtil.GetPreviewClip(value.project, value.entry) != null);
			});
			AddColumn("Generate", false, false, null, GetGenerateButton, (element, index) =>
			{
				var value = s_filteredEntries[index];
				var button = element as Button;
				button.userData = index;

				if (YarnParserUtil.TryGetSpeakerAndText(value.entry, out string speaker, out string text))
				{
					var speakerType = YarnEditorUtil.GetDialogueSpeakerTypes()
						.FirstOrDefault(x => string.Equals(speaker, x.name, StringComparison.OrdinalIgnoreCase));
					button.SetEnabled(speakerType?.ttsVoice != null);
				}
				else
				{
					button.SetEnabled(false);
				}
			});
			AddColumn("Match", true, false, null, GetToggle, (element, index) =>
			{
				(element as Toggle).value = s_filteredEntries[index].speakerTextMetadataMatch;
			});
			AddColumn("Project", true, true, null, GetObjectField, (element, index) =>
			{
				(element as ObjectField).value = s_filteredEntries[index].project;
			});
			AddColumn("File", true, true, null, GetObjectField, (element, index) =>
			{
				var assetPath = s_filteredEntries[index].entry.File.Substring(Application.dataPath.Length + 1);
				(element as ObjectField).value = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{assetPath}");
			});
			AddColumn("Node", true, true, null, null, (element, index) =>
			{
				(element as Label).text = s_filteredEntries[index].entry.Node;
			});
#if USE_UNITY_LOCALIZATION
			var locale = LocalizationEditorSettings.ActiveLocalizationSettings.GetSelectedLocale();
			if (locale != null)
			{
				AddColumn($"{locale.Identifier.Code} - Text", true, false, null, GetToggle, (element, index) =>
				{
					(element as Toggle).value = s_filteredEntries[index].stringInTable;
				});
				AddColumn($"{locale.Identifier.Code} - Audio", true, false, null, GetToggle, (element, index) =>
				{
					(element as Toggle).value = s_filteredEntries[index].audioInTable;
				});
			}
#endif

			root.Add(s_columnListView);
		}

		private void AddColumn(string name, bool sortable, bool stretchable, Comparison<int> comparison, Func<VisualElement> makeCell, Action<VisualElement, int> bindCell)
		{
			Column column = new Column()
			{
				name = name,
				title = name,
				sortable = sortable,
				stretchable = stretchable,
				comparison = comparison,
			};
			s_columnListView.columns.Add(column);

			if (makeCell == null)
			{
				makeCell = () => new Label();
			}
			column.makeCell = makeCell;
			column.bindCell = bindCell;
		}

		private VisualElement GetPreviewButton()
		{
			var icon = EditorGUIUtility.IconContent("PlayButton");
			var element = new Button()
			{
				iconImage = new Background()
				{
					texture = icon.image as Texture2D
				},
			};
			element.style.minHeight = element.style.maxHeight = BUTTON_HEIGHT;
			element.style.minWidth = element.style.maxWidth = BUTTON_WIDTH;
			element.RegisterCallback<ClickEvent>(PreviewButtonClicked);
			return element;
		}

		private VisualElement GetGenerateButton()
		{
			var icon = EditorGUIUtility.IconContent("Refresh");
			var element = new Button()
			{
				iconImage = new Background()
				{
					texture = AssetUtil.LoadFirstAsset<Texture2D>("GenerateTTS EditorIcon"),
				},
			};
			element.style.minHeight = element.style.maxHeight = BUTTON_HEIGHT;
			element.style.minWidth = element.style.maxWidth = BUTTON_WIDTH;
			element.RegisterCallback<ClickEvent>(GenerateButtonClicked);
			return element;
		}

		private VisualElement GetObjectField()
		{
			var element = new ObjectField();
			element.SetEnabled(false);
			return element;
		}

		private VisualElement GetToggle()
		{
			var element = new Toggle();
			element.SetEnabled(false);
			return element;
		}

#endregion

		#region Preview Methods

		private void PreviewButtonClicked(ClickEvent e)
		{
			var value = s_filteredEntries[(int)(e.target as Button).userData];
			AudioUtil.PlayPreviewClip(YarnEditorUtil.GetPreviewClip(value.project, value.entry));
		}

#endregion

		#region Generate Methods

		private void GenerateButtonClicked(ClickEvent e)
		{
			var value = s_filteredEntries[(int)(e.target as Button).userData];
			var clip = YarnEditorUtil.GetPreviewClip(value.project, value.entry);
			if (clip != null && !EditorUtility.DisplayDialog(
				"Generate AudioClip",
				$"This action will override {clip.name}. Are you sure that you want to perform this action?",
				"Override",
				"Cancel"))
			{
				return;
			}

			DialogueSettings.Generate(value.project, value.entry);
		}

		private void GenerateAllButtonClicked(ClickEvent e)
		{
			bool overrideAll = false;
			List<YarnStringEntry> entries = new();
			foreach (var value in s_filteredEntries)
			{
				if (!overrideAll)
				{
					var clip = YarnEditorUtil.GetPreviewClip(value.project, value.entry);
					if (clip != null)
					{
						int result = EditorUtility.DisplayDialogComplex(
							"Generate AudioClip",
							$"This action will override {clip.name}. Are you sure that you want to perform this action?",
							"Override",
							"Cancel",
							"Override All");
						if (result == 1)
							return;

						overrideAll = result == 2;
					}
				}
				entries.Add(value);
			}

			foreach (var g in entries.GroupBy(x => x.project))
			{
				DialogueSettings.Generate(g.Key, g.Select(x => x.entry));
			}
		}

		private void RefreshButtonClicked(ClickEvent e)
		{
			RefreshEntries();
		}

		#endregion

		#region Search Methods

		private static void Search(ChangeEvent<string> e)
		{
			Search(e.newValue);
		}

		public static void Search(string value)
		{
			s_filteredEntries.Clear();
			if (string.IsNullOrWhiteSpace(value))
			{
				s_filteredEntries.AddRange(s_entries);
			}
			else
			{
				foreach (var x in s_entries)
				{
					YarnParserUtil.TryGetSpeakerAndText(x.entry, out string speaker, out string text);
					string line = Regex.Replace(value, @"\w:\w*", string.Empty).Trim();

					if (text.Contains(line, StringComparison.InvariantCultureIgnoreCase)
						&& IsMatch('s', value, speaker)
						&& IsMatch('n', value, x.entry.Node)
						&& IsMatch('f', value, Path.GetFileNameWithoutExtension(x.entry.File))
						&& IsMatch('p', value, x.project.name)
						&& IsMatch('i', value, x.entry.ID)
						&& IsMatch('d', value, YarnParserUtil.GetMetadata(x.entry))
						&& IsMatch('m', value, (r) => Equals(r, x.speakerTextMetadataMatch)))
					{
						s_filteredEntries.Add(x);
					}
				}
			}

			s_columnListView?.Rebuild();
		}

		private static bool IsMatch(char key, string value, string find)
		{
			var match = Regex.Match(value, $"{key}" + @":(?<value>\w*)");
			if (!match.Success)
				return true;

			string search = match.Groups["value"].Value;
			if (string.IsNullOrWhiteSpace(search))
				return false;

			return find.Contains(search, StringComparison.InvariantCultureIgnoreCase);
		}

		private static bool IsMatch(char key, string value, Func<bool, bool> predicate)
		{
			var match = Regex.Match(value, $"{key}" + @":(?<value>\w*)");
			if (!match.Success)
				return true;

			string search = match.Groups["value"].Value;
			if (string.IsNullOrWhiteSpace(search)
				|| !bool.TryParse(search, out var result))
				return false;

			return predicate.Invoke(result);
		}

		#endregion

		#region Structures

		private struct YarnStringEntry
		{
			public YarnProject project;
			public Yarn.Unity.StringTableEntry entry;

			public bool speakerTextMetadataMatch;

#if USE_UNITY_LOCALIZATION
			public bool stringInTable;
			public bool audioInTable;
#endif
		}

		#endregion
	}
}