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
	}
#endif
}