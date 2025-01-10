using Meta.Voice.Audio;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Integrations;
using Meta.WitAi.TTS.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToolkitEngine.Dialogue;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	public class YarnViewer : EditorWindow
	{
		#region Fields

		private List<YarnProject> m_projects;
		private Dictionary<string, DialogueSpeakerType> m_speakerTypeMap = new();

		private MultiColumnHeaderState m_multiColumnHeaderState;
		private MultiColumnHeader m_multiColumnHeader;
		private MultiColumnHeaderState.Column[] m_columns;

		private Vector2 m_scrollPosition;
		private readonly Color m_lightColor = Color.white * 0.3f;
		private readonly Color m_darkColor = Color.white * 0.1f;

		private int m_offset;

		private readonly float ID_WIDTH = 80f;
		private readonly float BUTTON_WIDTH = EditorGUIUtility.singleLineHeight * 3f;

		#endregion

		#region Methods

		[MenuItem("Window/Yarn Spinner/Yarn Viewer")]
		public static void ShowWindow()
		{
			var window = GetWindow<YarnViewer>();
			window.titleContent = new GUIContent("Yarn Viewer");
		}

		private void Initialize()
		{
			// Find all YarnProjects that exists in database
			m_projects = AssetDatabase.FindAssets("t:YarnProject")
				.Select(x => AssetDatabase.LoadAssetAtPath<YarnProject>(AssetDatabase.GUIDToAssetPath(x)))
				.ToList();

			// Find all DialogueSpeakerTypes that exist in database, using CharacterName as key
			foreach (var guid in AssetDatabase.FindAssets("t:DialogueSpeakerType"))
			{
				var speakerType = AssetDatabase.LoadAssetAtPath<DialogueSpeakerType>(AssetDatabase.GUIDToAssetPath(guid));
				if (string.IsNullOrWhiteSpace(speakerType.characterName))
				{
					Debug.LogErrorFormat("DialogueSpeakerType {0} has undefined Character Name!", speakerType.name);
					continue;
				}

				m_speakerTypeMap.Add(speakerType.characterName, speakerType);
			}

			m_columns = new MultiColumnHeaderState.Column[]
			{
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = false, // At least one column must be there.
					autoResize = false,
					minWidth = ID_WIDTH,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("ID", "Line ID."),
					headerTextAlignment = TextAlignment.Center,
				},
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = false,
					minWidth = ID_WIDTH,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Speaker", "Speaker of line."),
					headerTextAlignment = TextAlignment.Center,
				},
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = false,
					minWidth = ID_WIDTH,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Line", "Text of line."),
					headerTextAlignment = TextAlignment.Center,
				},
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = true,
					minWidth = ID_WIDTH * 2f,
					maxWidth = ID_WIDTH * 2f,
					canSort = false,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Audio Clip", "Audio clip asset associated with line."),
					headerTextAlignment = TextAlignment.Center,
				},
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = true,
					minWidth = BUTTON_WIDTH,
					maxWidth = BUTTON_WIDTH,
					canSort = false,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Preview", "Preview audio clip asset associated with line."),
					headerTextAlignment = TextAlignment.Center,
				},
#if META_VOICE
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = false,
					minWidth = BUTTON_WIDTH,
					maxWidth = BUTTON_WIDTH,
					canSort = false,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent(string.Empty, "Generate audio clip asset associated with line."),
					headerTextAlignment = TextAlignment.Center,
				},
#endif
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = false,
					minWidth = ID_WIDTH,
					canSort = false,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Metadata", "Metadata associated with this line."),
					headerTextAlignment = TextAlignment.Center,
				},
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = false,
					minWidth = ID_WIDTH,
					canSort = false,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Yarn Project", "Yarn Project associated with this line."),
					headerTextAlignment = TextAlignment.Center,
				},
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = false,
					minWidth = ID_WIDTH,
					canSort = false,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Yarn Script", "Yarn Script associated with this line."),
					headerTextAlignment = TextAlignment.Center,
				},
				new MultiColumnHeaderState.Column()
				{
					allowToggleVisibility = true,
					autoResize = false,
					minWidth = ID_WIDTH,
					canSort = false,
					sortingArrowAlignment = TextAlignment.Right,
					headerContent = new GUIContent("Node", "Yarn Node associated with this line."),
					headerTextAlignment = TextAlignment.Center,
				},
			};

			m_multiColumnHeaderState = new MultiColumnHeaderState(m_columns);
			m_multiColumnHeader = new MultiColumnHeader(m_multiColumnHeaderState);
			m_multiColumnHeader.ResizeToFit();
		}

		private void OnGUI()
		{
			if (m_multiColumnHeader == null)
			{
				Initialize();
			}

			GUILayout.FlexibleSpace();
			Rect rect = GUILayoutUtility.GetLastRect();
			rect.width = position.width;
			rect.height = position.height;

			// Draw header
			Rect headerRect = new Rect(rect)
			{
				height = EditorGUIUtility.singleLineHeight,
			};
			m_multiColumnHeader.OnGUI(headerRect, 0f);

			// Draw scroll view
			float viewHeight = position.height - EditorGUIUtility.singleLineHeight;
			m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, false, true, GUILayout.Height(viewHeight));

			// Draw rows
			int rowIndex = 0;
			foreach (var project in m_projects)
			{
				DrawProject(project, headerRect, ref rowIndex);
			}

			EditorGUILayout.EndScrollView();
		}

		private void DrawProject(YarnProject project, Rect position, ref int rowIndex)
		{
			if (project == null)
				return;

			var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(project)) as YarnProjectImporter;
			if (importer == null)
				return;

			var entries = importer.GenerateStringsTable();

			foreach (var entry in entries)
			{
				if (string.IsNullOrWhiteSpace(entry.ID))
					continue;

				GUILayout.Label(string.Empty);

				int columnIndex = 0;
				Rect rowRect = new Rect(position);
				rowRect.y = EditorGUIUtility.singleLineHeight * rowIndex++;

				string id = string.Empty;
				var match = Regex.Match(entry.ID, @"^line:(?<id>.*)$");
				if (match.Success)
				{
					id = match.Groups["id"].Value;
				}

				// ID
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					EditorGUI.LabelField(cellRect, id);
				});

				string speaker = string.Empty, text = string.Empty;
				match = Regex.Match(entry.Text, @"^(?<speaker>\w*?): ?(?<text>.*)$");
				if (match.Success)
				{
					speaker = match.Groups["speaker"].Value;
					text = match.Groups["text"].Value;
				}

				// Speaker
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					EditorGUI.LabelField(cellRect, speaker);
				});

				// Line
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					EditorGUI.LabelField(cellRect, text);
				});

				// Audio clip
				var audioClip = project.baseLocalization.GetLocalizedObject<AudioClip>(entry.ID);
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(cellRect, audioClip, typeof(AudioClip), false);
					EditorGUI.EndDisabledGroup();
				});

				// Preview
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					if (audioClip != null)
					{
						var content = EditorGUIUtility.IconContent("PlayButton");
						content.tooltip = "Play";

						if (GUI.Button(cellRect, content))
						{
							PlayPreviewClip(audioClip);
						}
					}
				});

#if META_VOICE
				// Generate
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					if (!m_speakerTypeMap.TryGetValue(speaker, out var speakerType))
						return;

					var content = EditorGUIUtility.IconContent("preAudioAutoPlayOff");
					content.tooltip = "Generate";

					if (GUI.Button(cellRect, content))
					{
						if (audioClip != null)
						{
							if (!EditorUtility.DisplayDialog(
								"Generate AudioClip",
								"Thiis action will override an existing asset. Are you sure that you want to perform this action?",
								"Yes",
								"No"))
							{
								return;
							}
						}

						GenerateClip(project, entry, id, text, speakerType.voiceSettings);
					}
				});
#endif

				// Yarn Project
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					string text = string.Empty;
					string[] metadata = project.lineMetadata.GetMetadata(entry.ID);
					if (metadata != null)
					{
						text = string.Join(", ", metadata);
					}

					EditorGUI.LabelField(cellRect, text);
				});

				// Yarn Project
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(cellRect, project, typeof(YarnProject), false);
					EditorGUI.EndDisabledGroup();
				});

				// Yarn Script
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					var assetPath = entry.File.Substring(Application.dataPath.Length + 1);
					var file = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{assetPath}");

					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.ObjectField(cellRect, file, typeof(TextAsset), false);
					EditorGUI.EndDisabledGroup();
				});

				// Yarn Node
				DrawColumn(rowRect, columnIndex++, (cellRect) =>
				{
					EditorGUI.LabelField(cellRect, entry.Node);
				});
			}

			var lineIds = project.baseLocalization.GetLineIDs();
			switch (m_multiColumnHeader.sortedColumnIndex)
			{
				// ID
				case 0:
					lineIds = m_multiColumnHeader.IsSortedAscending(0)
						? lineIds.OrderBy(x => x)
						: lineIds.OrderByDescending(x => x);
					break;

				// Speaker
				case 1:
					lineIds = GetOrderLocalizedLines(project, lineIds, 1, "speaker");
					break;

				// Line
				case 2:
					lineIds = GetOrderLocalizedLines(project, lineIds, 2, "line");
					break;
			}
		}

		private void DrawColumn(Rect position, int columnIndex, System.Action<Rect> drawCell)
		{
			if (m_multiColumnHeader.IsColumnVisible(columnIndex))
			{
				int visibleColumnIndex = m_multiColumnHeader.GetVisibleColumnIndex(columnIndex);
				Rect columnRect = m_multiColumnHeader.GetColumnRect(visibleColumnIndex);

				columnRect.y = position.y;

				drawCell(m_multiColumnHeader.GetCellRect(visibleColumnIndex, columnRect));
			}
		}

		private IEnumerable<string> GetOrderLocalizedLines(YarnProject project, IEnumerable<string> lineIds, int columnIndex, string groupKey)
		{
			var unorderedLineData = lineIds.Select(x => new
			{
				lineId = x,
				match = Regex.Match(project.baseLocalization.GetLocalizedString(x), @"^(?<speaker>\w*?):(?<line>.*$)")
			})
			.Where(x => x.match.Success);

			if (m_multiColumnHeader.IsSortedAscending(columnIndex))
			{
				return unorderedLineData.OrderBy(x => x.match.Groups[groupKey].Value)
					.Select(x => x.lineId);
			}
			else
			{
				return unorderedLineData.OrderByDescending(x => x.match.Groups[groupKey].Value)
					.Select(x => x.lineId);
			}
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

		private async void GenerateClip(YarnProject project, StringTableEntry entry, string id, string text, TTSWitVoiceSettings voiceSettings)
		{
			var service = GameObject.FindFirstObjectByType<TTSService>();

			var requestData = CreateRequest(project, entry, id);
			requestData.ClipData = service.GetClipData(text, voiceSettings, null);

			var errors = await service.LoadAsync(requestData.ClipData, requestData.OnReady);
		}

		private TTSSpeakerRequestData CreateRequest(YarnProject project, StringTableEntry entry, string id)
		{
			TTSSpeakerRequestData requestData = new TTSSpeakerRequestData();
			requestData.OnReady = (clipData) =>
			{
				if (clipData.clipStream is RawAudioClipStream rawAudioClipStream)
				{
					AudioClip audioClip = AudioClip.Create(
						id,
						rawAudioClipStream.SampleBuffer.Length,
						rawAudioClipStream.Channels,
						rawAudioClipStream.SampleRate,
						false,
						(samples) =>
						{
							// Length of copied samples
							var length = 0;

							// Copy as many samples as possible from the raw sample buffer
							var start = m_offset;
							var available = Mathf.Max(0, rawAudioClipStream.AddedSamples - start);
							length = Mathf.Min(samples.Length, available);

							if (length > 0)
							{
								Array.Copy(rawAudioClipStream.SampleBuffer, start, samples, 0, length);
								m_offset += length;
							}

							// Clear unavailable samples
							if (length < samples.Length)
							{
								int difference = samples.Length - length;
								Array.Clear(samples, length, difference);
								m_offset += difference;
							}
						},
						(offset) =>
						{
							m_offset = offset;
						});

					var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(project)) as YarnProjectImporter;
					var assetPath = AssetDatabase.GetAssetPath(importer.ImportData.BaseLocalizationEntry.assetsFolder);
					assetPath = assetPath.Substring("Assets/".Length);

					AudioUtility.Save($"{Application.dataPath}/{assetPath}/{Path.GetFileNameWithoutExtension(entry.File)}/{id}.wav", audioClip, true);
					AssetDatabase.Refresh();
					
					importer.SaveAndReimport();
				}
			};
			requestData.IsReady = false;
			requestData.StartTime = DateTime.UtcNow;
			requestData.PlaybackCompletion = new TaskCompletionSource<bool>();
			requestData.PlaybackEvents = new TTSSpeakerClipEvents();
			requestData.StopPlaybackOnLoad = true;

			return requestData;
		}

		#endregion

		#region Unity Callbacks

		private void Awake()
		{
			Initialize();
		}

		private void OnDestroy()
		{
			StopAllPreviewClips();
		}

		#endregion

		#region Structures

		private class TTSSpeakerRequestData
		{
			public TTSClipData ClipData;
			public Action<TTSClipData> OnReady;
			public bool IsReady;
			public string Error;
			public DateTime StartTime;
			public bool StopPlaybackOnLoad;
			public TTSSpeakerClipEvents PlaybackEvents;
			public TaskCompletionSource<bool> PlaybackCompletion;
		}

		#endregion
	}
}