using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Dialogue/Portrait Set", order = 1600)]
	public class PortraitSet : ScriptableObject
    {
		#region Fields

		[SerializeField]
		private PortraitMap m_frames;

		#endregion

		#region Methods

		public bool HasPortrait(string key) => m_frames.ContainsKey(key);
		public Sprite GetPortrait(string key) => m_frames[key];
		public bool TryGetPortrait(string key, out Sprite portrait) => m_frames.TryGetValue(key, out portrait);

		#endregion
	}

	#region Structures

	[System.Serializable]
	public class PortraitMap : SerializableDictionary<string, Sprite>
	{ }

	#endregion
}