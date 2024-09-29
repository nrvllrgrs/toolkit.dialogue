using System.Collections.Generic;
using UnityEngine;
using Yarn;

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Config/DialogueManager Config")]
	public class DialogueManagerConfig : ScriptableObject
    {
		#region Fields

		[SerializeField, Nested]
		private List<DialogueCategory> m_categories = new();

		#endregion

		#region Properties

		public DialogueCategory[] categories => m_categories.ToArray();

		#endregion
	}
}