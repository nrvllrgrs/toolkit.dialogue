using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Dialogue/Dialogue Speaker")]
	public class DialogueSpeakerType : ScriptableObject
    {
		#region Fields

		[SerializeField, Tooltip("Name of character that is speaking.")]
		private string m_characterName;

		#endregion

		#region Properties

		public string characterName => m_characterName;

		#endregion
	}
}
