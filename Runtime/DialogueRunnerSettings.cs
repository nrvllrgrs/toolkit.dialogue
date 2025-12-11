using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class DialogueRunnerSettings : MonoBehaviour
    {
		#region Enumerators

		public enum RunSelectedOption
		{
			UseDialogueRunner,
			AsLine,
			NotAsLine
		};

		#endregion

		#region Fields

		[SerializeField]
		private DialogueRegistration m_registration;

		[SerializeField]
		private VariableStorageBehaviour m_variableStorage;

		[SerializeField, FormerlySerializedAs("m_dialogueViews")]
		private DialoguePresenterBase[] m_dialoguePresenters;

		[SerializeField]
		private RunSelectedOption m_runSelectedOption = RunSelectedOption.UseDialogueRunner;

		#endregion

		#region Properties

		public DialogueRegistration registration => m_registration;
		public VariableStorageBehaviour variableStorage => m_variableStorage;
		public DialoguePresenterBase[] dialoguePresenters => m_dialoguePresenters;
		public RunSelectedOption runSelectedOption => m_runSelectedOption;

		public DialogueRunner firstDialogueRunner
		{
			get
			{
				return DialogueManager.CastInstance.TryGetFirstDialogueRunner(registration, out var dialogueRunner)
					? dialogueRunner
					: null;
			}
		}

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

		public void Stop()
		{
			firstDialogueRunner?.Stop();
		}

		#endregion
	}
}