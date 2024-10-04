using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Dialogue/Nudge Type")]
	public class NudgeType : ScriptableObject
    {
		#region Fields

		[SerializeField, Min(0f), Tooltip("Seconds to wait between nudges.")]
		private float m_delayTime = 60f;

		[SerializeField, Min(0f), Tooltip("Minimum seconds to wait after unpaused.")]
		private float m_minDelayTime = 60f;

		[SerializeField, Tooltip("Name of index variable used to increment nudges. Automatically reset when new nudge started.")]
		private string m_indexVarName = string.Empty;

		[SerializeField, Tooltip("Indicates whether nudge collection should clear stack when set.")]
		private bool m_autoClear = false;

		#endregion

		#region Properties

		/// <summary>
		/// Seconds to wait between nudges.
		/// </summary>
		public float delayTime => m_delayTime;

		/// <summary>
		/// Minimum seconds to wait after unpaused.
		/// </summary>
		public float minDelayTime => m_minDelayTime;

		/// <summary>
		/// Name of index variable used to increment nudges. Automatically reset when new nudge started.
		/// </summary>
		public string indexVarName => m_indexVarName;

		/// <summary>
		/// Indicates whether nudge collection should clear stack when set.
		/// </summary>
		public bool autoClear => m_autoClear;

		#endregion
	}
}