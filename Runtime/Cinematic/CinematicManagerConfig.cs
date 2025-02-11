using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Config/CinematicManager Config", order = 10)]
	public class CinematicManagerConfig : ScriptableObject
    {
		#region Fields

		[SerializeField, Tooltip("DialogueType associated with cinematics.")]
		private DialogueType m_dialogueType;

		#endregion

		#region Properties

		public DialogueType dialogueType => m_dialogueType;

		#endregion
	}
}