using UnityEngine;

#if USE_UNITY_LOCALIZATION
using UnityEngine.Localization;
using Yarn.Unity;
#endif

namespace ToolkitEngine.Dialogue
{
#if USE_UNITY_LOCALIZATION
	[CreateAssetMenu(menuName = "Toolkit/Dialogue/Localized Table Map")]
    public class LocalizedTableMap : ScriptableObject
    {
		#region Fields

		[SerializeField]
		private LocalizedMap m_map;

		#endregion

		#region Methods

		public LocalizedTables GetTables(YarnProject project) => m_map[project];
		public bool ContainsProject(YarnProject project) => m_map.ContainsKey(project);
		public bool TryGetTables(YarnProject project, out LocalizedTables tables) => m_map.TryGetValue(project, out tables);

		#endregion
	}

	[System.Serializable]
	public struct LocalizedTables
	{
		public LocalizedStringTable stringTable;
		public LocalizedAssetTable audioTable;
		public LocalizedAssetTable animationTable;
	}

	[System.Serializable]
	public class LocalizedMap : SerializableDictionary<YarnProject, LocalizedTables>
	{ }

#endif
}