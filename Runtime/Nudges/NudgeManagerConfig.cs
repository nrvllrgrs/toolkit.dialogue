using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Dialogue
{
	[CreateAssetMenu(menuName = "Toolkit/Config/NudgeManager Config")]
	public class NudgeManagerConfig : ScriptableObject, IInstantiableSubsystemConfig
    {
		#region Fields

		[SerializeField]
		private DialogueRunnerControl m_template;

		[SerializeField]
		private DialogueType m_dialogueType;

		[SerializeField, Nested]
		private List<NudgeType> m_priorities = new();

		#endregion

		#region Properties

		public DialogueType dialogueType => m_dialogueType;

		#endregion

		#region Methods

		public int GetPriority(NudgeType nudgeType)
		{
			int index = m_priorities.IndexOf(nudgeType);
			return index >= 0
				? (m_priorities.Count - 1) - index
				: index;
		}

		#endregion

		#region IInstantiableSubsystemConfig Methods

		public GameObject GetTemplate() => m_template?.gameObject;
		public Type GetManagerType() => typeof(NudgeManager);

		#endregion
	}
}