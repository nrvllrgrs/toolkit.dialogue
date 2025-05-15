using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class DialogueRunnerSettings : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		private DialogueRegistration m_registration;

		[SerializeField]
		private VariableStorageBehaviour m_variableStorage;

		[SerializeField]
		private DialogueViewBase[] m_dialogueViews;

		#endregion

		#region Properties

		public DialogueRegistration registration => m_registration;
		public VariableStorageBehaviour variableStorage => m_variableStorage;
		public DialogueViewBase[] dialogueViews => m_dialogueViews;

		#endregion

		#region Methods

		private void OnEnable()
		{
			DialogueManager.CastInstance.Register(this);
		}

		private void OnDisable()
		{
			if (DialogueManager.Exists)
			{
				DialogueManager.CastInstance.Unregister(this);
			}
		}

		#endregion
	}
}