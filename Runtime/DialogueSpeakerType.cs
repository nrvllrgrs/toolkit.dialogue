using UnityEngine;

#if USE_UNITY_LOCALIZATION
using UnityEngine.Localization;
#endif

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Dialogue/Dialogue Speaker")]
	public class DialogueSpeakerType : ScriptableObject
    {
		#region Fields

#if USE_UNITY_LOCALIZATION
		[SerializeField, Tooltip("Name of speaking character.")]
		private LocalizedString m_displayName;
#endif

		[SerializeField]
		private Color m_color = Color.white;

		[SerializeField]
		private AnimationSet m_animationSet;

#if UNITY_EDITOR
		[SerializeField]
		private TTSVoice m_ttsVoice;
#endif

		#endregion

		#region Properties

		public string displayName
		{
			get
			{
#if USE_UNITY_LOCALIZATION
				try
				{
					return m_displayName.GetLocalizedString();
				}
				catch { }
#else
#endif
				return name;
			}
		}

		public Color color => m_color;
		public AnimationSet animationSet => m_animationSet;

#if UNITY_EDITOR
		public TTSVoice ttsVoice => m_ttsVoice;
#endif
		#endregion
	}
}
