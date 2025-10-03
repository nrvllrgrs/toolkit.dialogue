using UnityEngine;
using UnityEngine.Serialization;
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

		[SerializeField, FormerlySerializedAs("m_dialogueViews")]
		private DialoguePresenterBase[] m_dialoguePresenters;

		#endregion

		#region Properties

		public DialogueRegistration registration => m_registration;
		public VariableStorageBehaviour variableStorage => m_variableStorage;
		public DialoguePresenterBase[] dialoguePresenters => m_dialoguePresenters;

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