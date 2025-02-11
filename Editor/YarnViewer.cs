using Gilzoide.EasyProjectSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ToolkitEngine.Dialogue;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	public class YarnViewer : EditorWindow
	{
		#region Fields

		private static HashSet<YarnStringEntry> s_entries = new();
		private static List<YarnStringEntry> s_filteredEntries = new();

		private static HashSet<StringTableCollection> s_stringTableCollections = new();
		private static HashSet<AssetTableCollection> s_assetTableCollections = new();

		private static YarnViewer s_window;
		private static ToolbarSearchField s_searchField;
		private static MultiColumnListView s_columnListView;

		#endregion

		#region Methods

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
			s_stringTableCollections.Clear();
			s_assetTableCollections.Clear();

			foreach (var project in AssetDatabase.FindAssets("t:YarnProject")
				.Select(x => AssetDatabase.LoadAssetAtPath<YarnProject>(AssetDatabase.GUIDToAssetPath(x))))
			{
				var importer = GetImporter(project);
				if (importer == null)
					continue;

				foreach (var entry in importer.GenerateStringsTable())
				{
					s_entries.Add(new YarnStringEntry()
					{
						project = project,
						entry = entry,
					});
				}

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
			}

			Search(s_searchField?.value ?? string.Empty);
		}

		private static YarnProjectImporter GetImporter(YarnProject project)
		{
			return AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(project)) as YarnProjectImporter;
		}

		private void OnDestroy()
		{
			StopAllPreviewClips();
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
				var generateAllButton = new Button()
				{
					iconImage = new Background()
					{
						texture = icon.image as Texture2D
					},
					tooltip = "Generate All",
				};
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

			AddColumn("ID", true, null, null, (element, index) =>
			{
				(element as Label).text = YarnParserUtility.GetID(s_filteredEntries[index].entry);
			});
			AddColumn("Speaker", true, null, null, (element, index) =>
			{
				YarnParserUtility.TryGetSpeakerAndText(s_filteredEntries[index].entry, out string speaker, out string text);
				(element as Label).text = speaker;
			});
			AddColumn("Text", true, null, null, (element, index) =>
			{
				YarnParserUtility.TryGetSpeakerAndText(s_filteredEntries[index].entry, out string speaker, out string text);
				(element as Label).text = text;
			});
			AddColumn("Metadata", true, null, null, (element, index) =>
			{
				(element as Label).text = YarnParserUtility.GetMetadata(s_filteredEntries[index].entry);
			});
			AddColumn("Preview", false, null, GetPreviewButton, (element, index) =>
			{
				(element as Button).userData = index;
			});
			AddColumn("Generate", false, null, GetGenerateButton, (element, index) =>
			{
				(element as Button).userData = index;
			});
			AddColumn("Project", true, null, GetObjectField, (element, index) =>
			{
				(element as ObjectField).value = s_filteredEntries[index].project;
			});
			AddColumn("File", true, null, GetObjectField, (element, index) =>
			{
				var assetPath = s_filteredEntries[index].entry.File.Substring(Application.dataPath.Length + 1);
				(element as ObjectField).value = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{assetPath}");
			});
			AddColumn("Node", true, null, null, (element, index) =>
			{
				(element as Label).text = s_filteredEntries[index].entry.Node;
			});
#if USE_UNITY_LOCALIZATION
			foreach (var locale in LocalizationEditorSettings.GetLocales())
			{
				AddColumn($"{locale.Identifier.Code} - Text", true, null, GetToggle, (element, index) =>
				{
					var value = s_filteredEntries[index];
					var record = s_stringTableCollections.Select(x => x.GetTable(locale.Identifier) as StringTable)
						.Select(x => x.GetEntry(value.entry.ID))
						.Where(x => x != null)
						.FirstOrDefault();
					(element as Toggle).value = !string.IsNullOrWhiteSpace(record?.Value);
				});
				AddColumn($"{locale.Identifier.Code} - Audio", true, null, GetToggle, (element, index) =>
				{
					var value = s_filteredEntries[index];
					var record = s_stringTableCollections.Select(x => x.GetTable(locale.Identifier) as StringTable)
						.Select(x => x.GetEntry(value.entry.ID))
						.Where(x => x != null)
						.FirstOrDefault();
					(element as Toggle).value = GetPreviewClip(s_filteredEntries[index], locale.Identifier) != null;
				});
			}
#endif

			root.Add(s_columnListView);
		}

		private void AddColumn(string name, bool sortable, Comparison<int> comparison, Func<VisualElement> makeCell, Action<VisualElement, int> bindCell)
		{
			Column column = new Column()
			{
				name = name,
				title = name,
				sortable = sortable,
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
				}
			};
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
					texture = icon.image as Texture2D
				}
			};
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
			PlayPreviewClip(GetPreviewClip(s_filteredEntries[(int)(e.target as Button).userData]));
		}

		private AudioClip GetPreviewClip(YarnStringEntry value)
		{
			LocaleIdentifier identifier;
			switch (value.project.localizationType)
			{
				case LocalizationType.YarnInternal:
					identifier = value.project.baseLocalization?.LocaleCode;
					break;

				case LocalizationType.Unity:
					identifier = LocalizationEditorSettings.ActiveLocalizationSettings.GetSelectedLocale().Identifier;
					break;
			}
			return GetPreviewClip(value, identifier);
		}

		private AudioClip GetPreviewClip(YarnStringEntry value, LocaleIdentifier identifier)
		{
			AudioClip clip = null;
			switch (value.project.localizationType)
			{
				case LocalizationType.YarnInternal:
					clip = value.project.GetLocalization(identifier.Code)?.GetLocalizedObject<AudioClip>(value.entry.ID);
					break;

				case LocalizationType.Unity:
					var activeLocalization = LocalizationEditorSettings.ActiveLocalizationSettings;
					var record = s_assetTableCollections.Select(x => x.GetTable(identifier) as AssetTable)
						.Select(x => x.GetEntry(value.entry.ID))
						.Where(x => x != null)
						.FirstOrDefault();

					if (record != null)
					{
						clip = activeLocalization.GetAssetDatabase()?.GetLocalizedAsset<AudioClip>(record.Table.TableCollectionName, record.KeyId);
					}
					break;
			}
			return clip;
		}

		private void PlayPreviewClip(AudioClip audioClip)
		{
			StopAllPreviewClips();

			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

			MethodInfo method = audioUtilClass.GetMethod(
				"PlayPreviewClip",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
				null
			);
			method?.Invoke(null, new object[] { audioClip, 0, false });
		}

		private void StopAllPreviewClips()
		{
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

			MethodInfo method = audioUtilClass.GetMethod(
				"StopAllPreviewClips",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new Type[] { },
				null
			);
			method?.Invoke(null, new object[] { });
		}

		#endregion

		#region Generate Methods

		private void GenerateButtonClicked(ClickEvent e)
		{
			var value = s_filteredEntries[(int)(e.target as Button).userData];
			var clip = GetPreviewClip(value);
			if (clip != null && !EditorUtility.DisplayDialog(
				"Generate AudioClip",
				$"This action will override {clip.name}. Are you sure that you want to perform this action?",
				"Override",
				"Cancel"))
			{
				return;
			}

			ProjectSettings.Load<DialogueSettings>().Generate(value.project, value.entry);
		}

		private void GenerateAllButtonClicked(ClickEvent e)
		{
			bool overrideAll = false;
			List<YarnStringEntry> entries = new();
			foreach (var value in s_filteredEntries)
			{
				if (!overrideAll)
				{
					var clip = GetPreviewClip(value);
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

			var settings = ProjectSettings.Load<DialogueSettings>();
			foreach (var g in entries.GroupBy(x => x.project))
			{
				settings.Generate(g.Key, g.Select(x => x.entry));
			}
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
					YarnParserUtility.TryGetSpeakerAndText(x.entry, out string speaker, out string text);
					string line = Regex.Replace(value, @"\w:\w*", string.Empty).Trim();

					if (text.Contains(line, StringComparison.InvariantCultureIgnoreCase)
						&& IsMatch('s', value, speaker)
						&& IsMatch('n', value, x.entry.Node)
						&& IsMatch('f', value, Path.GetFileNameWithoutExtension(x.entry.File))
						&& IsMatch('p', value, x.project.name)
						&& IsMatch('i', value, x.entry.ID)
						&& IsMatch('d', value, YarnParserUtility.GetMetadata(x.entry)))
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
			if (!string.IsNullOrWhiteSpace(search))
				return true;

			return find.Contains(search, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		#region Structures

		private struct YarnStringEntry
		{
			public YarnProject project;
			public Yarn.Unity.StringTableEntry entry;
		}

		#endregion
	}
}