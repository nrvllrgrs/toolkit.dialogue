using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class DialogueRunnerSettings : MonoBehaviour
    {
		#region Enumerators

		public enum RegistrationMode
		{
			Category,
			Type
		};

		#endregion

		#region Fields

		[SerializeField]
		private RegistrationMode m_registration = RegistrationMode.Category;

		[SerializeField]
		private DialogueCategory m_dialogueCategory;

		[SerializeField]
		private DialogueType m_dialogueType;

		[SerializeField]
		private VariableStorageBehaviour m_variableStorage;

		[SerializeField]
		private DialogueViewBase[] m_dialogueViews;

		[SerializeField]
		private LineProviderBehaviour m_lineProvider;

		#endregion

		#region Properties

		public RegistrationMode registration => m_registration;
		public DialogueCategory dialogueCategory => m_dialogueCategory;
		public DialogueType dialogueType => m_dialogueType;
		public VariableStorageBehaviour variableStorage => m_variableStorage;
		public DialogueViewBase[] dialogueViews => m_dialogueViews;
		public LineProviderBehaviour lineProvider => m_lineProvider;

		#endregion

		#region Methods

		private void OnEnable()
		{
			DialogueManager.CastInstance.Register(this);
		}

		private void OnDisable()
		{
			DialogueManager.CastInstance.Unregister(this);
		}

		#endregion
	}
}