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
		private List<DialogueSpeakerType> m_speakers = new();

		[SerializeField]
		private Spawner m_dialogueSpawner;

		[SerializeField]
		private GameObject m_runnerSettingsTemplate;

#if USE_UNITY_LOCALIZATION
		[SerializeField]
		private LocalizedTableMap m_tableMap;
#endif
		#endregion

		#region Properties

		public DialogueCategory[] categories => m_categories.ToArray();
		public DialogueSpeakerType[] speakers => m_speakers.ToArray();
		public Spawner dialogueSpawner => m_dialogueSpawner;
		public GameObject runnerSettingsTemplate => m_runnerSettingsTemplate;

#if USE_UNITY_LOCALIZATION
		public LocalizedTableMap tableMap => m_tableMap;
#endif

		#endregion
	}
}