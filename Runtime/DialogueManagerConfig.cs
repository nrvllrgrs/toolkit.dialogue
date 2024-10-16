using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Config/DialogueManager Config")]
	public class DialogueManagerConfig : ScriptableObject
    {
		#region Fields

		[SerializeField, Nested]
		private List<DialogueCategory> m_categories = new();

		[SerializeField]
		private Spawner m_spawner;

		#endregion

		#region Properties

		public DialogueCategory[] categories => m_categories.ToArray();

		public Spawner dialogueSpawner => m_spawner;

		#endregion
	}
}