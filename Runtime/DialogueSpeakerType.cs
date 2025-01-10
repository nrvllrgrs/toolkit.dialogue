using UnityEngine;

#if UNITY_EDITOR && META_VOICE
using Meta.WitAi.TTS.Integrations;
#endif

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Dialogue/Dialogue Speaker")]
	public class DialogueSpeakerType : ScriptableObject
    {
		#region Fields

		[SerializeField, Tooltip("Name of character that is speaking.")]
		private string m_characterName;

#if UNITY_EDITOR && META_VOICE

		[SerializeField]
		public TTSWitVoiceSettings voiceSettings;

#endif
		#endregion

		#region Properties

		public string characterName => m_characterName;

		#endregion
	}
}
