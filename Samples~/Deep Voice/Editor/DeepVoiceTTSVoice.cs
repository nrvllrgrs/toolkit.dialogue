using ToolkitEngine.Dialogue;
using UnityEngine;
using AiKodexDeepVoice;

namespace PrometheusEditor.Dialogue
{
	[CreateAssetMenu(
		fileName = "DeepVoice TTS Voice",
		menuName = "Toolkit/Dialogue/TTS/DeepVoice/TTS Voice")]
	public class DeepVoiceTTSVoice : TTSVoice
    {
		#region Fields

		[SerializeField]
		private DeepVoiceEditor.Model m_model;

		[SerializeField]
		private DeepVoiceEditor.Voice m_voice;

		#endregion

		#region Properties

		public override string voiceName => GetVoiceName(m_voice.ToString());
		public DeepVoiceEditor.Model model => m_model;
		public DeepVoiceEditor.Voice voice => m_voice;

		#endregion
	}
}