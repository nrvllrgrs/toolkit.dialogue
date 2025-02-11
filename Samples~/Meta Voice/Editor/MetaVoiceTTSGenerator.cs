#if META_VOICE
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Data;
#endif

using NAudio.Wave;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ToolkitEngine.Dialogue;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Dialogue/TTS/Meta Voice Generator")]
    public class MetaVoiceTTSGenerator : TTSGenerator
    {
		#region Fields

#if META_VOICE
		[SerializeField]
		private TTSService m_service;
#endif

		private bool m_downloaded;

		#endregion

		#region Methods

		public override void Generate(YarnProject project, IEnumerable<StringTableEntry> entries)
		{
#if META_VOICE
			// Create map of all DialogueSpeakers
			Dictionary<string, DialogueSpeakerType> speakerMap = new();
			foreach (var guid in AssetDatabase.FindAssets("t:DialogueSpeakerType"))
			{
				var asset = AssetDatabase.LoadAssetAtPath<DialogueSpeakerType>(AssetDatabase.GUIDToAssetPath(guid));
				speakerMap.Add(asset.name, asset);
			}

			var service = Instantiate(m_service);
			service.hideFlags = HideFlags.HideAndDontSave;

			var projectImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(project)) as YarnProjectImporter;
			var assetPath = AssetDatabase.GetAssetPath(projectImporter.ImportData.BaseLocalizationEntry.assetsFolder);

			List<string> paths = new();
			foreach (var entry in entries)
			{
				Generate(entry, speakerMap, service, assetPath);
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			if (Application.isPlaying)
			{
				Destroy(service.gameObject);
			}
			else
			{
				DestroyImmediate(service.gameObject);
			}
#endif
		}

#if META_VOICE
		private async void Generate(StringTableEntry entry, Dictionary<string, DialogueSpeakerType> speakerMap, TTSService service, string directoryPath)
		{
			string id = YarnParserUtility.GetID(entry);
			if (!YarnParserUtility.TryGetSpeakerAndText(entry, out string speaker, out string text))
			{
				Debug.LogError($"Speaker is undefined for line \"{text}\"!");
				return;
			}

			if (!speakerMap.TryGetValue(speaker, out DialogueSpeakerType speakerType))
			{
				Debug.LogError($"DialogueSpeakerType {speaker} is undefined!");
				return;
			}

			var cacheSettings = new TTSDiskCacheSettings()
			{
				DiskCacheLocation = TTSDiskCacheLocation.Temporary,
			};

			m_downloaded = false;
			service.DownloadToDiskCache(text, speakerType.voiceSettings, cacheSettings, (clipData, path, error) =>
			{
				try
				{
					string dstPath = $"{directoryPath}/{Path.GetFileNameWithoutExtension(entry.File)}/{id}.wav";
					if (File.Exists(dstPath))
					{
						File.Delete(dstPath);
					}
					else
					{
						var dstDirectory = Path.GetDirectoryName(dstPath);
						if (!Directory.Exists(dstDirectory))
						{
							Directory.CreateDirectory(dstDirectory);
						}
					}

					using (var rawReader = new RawSourceWaveStream(File.OpenRead(path), new WaveFormat(clipData.clipStream.SampleRate, clipData.clipStream.Channels)))
					{
						WaveFileWriter.CreateWaveFile(dstPath, rawReader);
					}
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
				}
				m_downloaded = true;

			});

			await Task.Run(() =>
			{
				while (!m_downloaded)
				{
					Task.Delay(1000).Wait();
				}
			});
		}

#endif
		#endregion
	}
}