using System;
using System.Collections;
using System.IO;
using System.Text;
using ToolkitEditor.Dialogue;
using ToolkitEngine.Dialogue;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Yarn.Unity;
using static AiKodexDeepVoice.DeepVoiceEditor;

namespace PrometheusEditor.Dialogue
{
	[CreateAssetMenu(
		fileName = "DeepVoice TTS Generator",
		menuName = "Toolkit/Dialogue/TTS/DeepVoice/TTS Generator")]
	public class DeepVoiceTTSGenerator : TTSGenerator<DeepVoiceTTSVoice>
	{
		#region Fields

		[SerializeField]
		private string m_invoice;

		#endregion

		#region Methods

		protected override IEnumerator AsyncGenerate(YarnProject project, StringTableEntry entry, string text, DeepVoiceTTSVoice ttsVoice, Action<string> callback)
		{
			text = text.Replace("\"", string.Empty);
			string emotion = "shouting";

			switch (ttsVoice.model)
			{
				case Model.DeepVoice_Mono:
				case Model.DeepVoice_Multi:
					yield return EditorCoroutineUtility.StartCoroutine(
						Post(
							project,
							"http://50.19.203.25:5000/invoice",
							$"{{\"text\":\"{text}\", \"emotion\":\"{emotion}\", \"model\":\"{ttsVoice.model}\", \"invoice\":\"{m_invoice}\", \"name\":\"{ttsVoice.voice}\", \"variability\":\"{0.3f}\", \"clarity\":\"{0.75f}\"}}",
							YarnParserUtil.GetID(entry),
							callback),
						this);
					break;

				case Model.DeepVoice_Standard:
					break;

				case Model.DeepVoice_Neural:
					break;
			}
		}

		private IEnumerator Post(YarnProject project, string url, string bodyJsonString, string lineId, Action<string> callback)
		{
			var request = new UnityWebRequest(url, "POST");
			byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.Log("There was an error in generating the voice. Please check your invoice/order number and try again or check the documentation for more information.");
				if (request.responseCode == 400)
				{
					Debug.Log("Error in text field: Please check your prompt for quotes (\"\") and line breaks at the end of the prompt. There could also be special formatting in your text. Please remove any special formatting by pasting as plain text in a notepad and then pasting the text here. Inclusion of any special formatting or illegal characters will result in an error such as this. For best results, please use a combination of letters, periods and commas and make sure there are no line breaks in between or at the end. If you must use quotes or line breaks, please prepend them with a backslash. Please do not press enter in the text field before clicking on generate.");
				}
			}
			else
			{
				if (request.responseCode == 400)
				{
					Debug.Log("Error in text field: Please check your prompt for quotes (\"\") and line breaks at the end of the prompt. There could also be special formatting in your text. Please remove any special formatting by pasting as plain text in a notepad and then pasting the text here. Inclusion of any special formatting or illegal characters will result in an error such as this. For best results, please use a combination of letters, periods and commas and make sure there are no line breaks in between or at the end. If you must use quotes or line breaks, please prepend them with a backslash. Please do not press enter in the text field before clicking on generate.");
				}

				if (request.downloadHandler.text == "Invalid Response")
				{
					Debug.Log("Invalid Invoice/Order Number. Please check your invoice/order number and try again.");
				}
				else if (request.downloadHandler.text == "Limit Reached")
				{
					Debug.Log("It seems that you may have reached the limit. To check your character usage, please click on the Status button. Please wait until 30th/31st of the month to get a renewed character count. Thank you for using DeepVoice.");
				}
				else
				{
					string tempFileName = Path.GetTempFileName();
					byte[] soundBytes = Convert.FromBase64String(request.downloadHandler.text);
					File.WriteAllBytes(tempFileName, soundBytes);

					if (m_importAssets)
					{
						string mp3File = Path.Combine(ToolkitEditor.FileUtil.GetAbsolutePath(m_directory), lineId + ".mp3");
						ToolkitEditor.FileUtil.Move(tempFileName, mp3File, true);

						// Reimport project to sync audio asset
						AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(project), ImportAssetOptions.ImportRecursive);
						callback?.Invoke(mp3File);
					}
				}
			}

			request.Dispose();
		}

		#endregion
	}
}