using UnityEngine;

namespace ToolkitEngine.Dialogue
{
#if UNITY_EDITOR
	public abstract class TTSVoice : ScriptableObject
    {
		#region Fields

		[SerializeField, TextArea(3, 3)]
		private string m_previewText;

		#endregion

		#region Properties

		public abstract string voiceName { get; }

		#endregion

		#region Methods

		protected string GetVoiceName(string voiceName) => $"{voiceName}+{GetType().Name}";

		#endregion
	}
#endif
}