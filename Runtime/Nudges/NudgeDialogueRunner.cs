using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	[RequireComponent(typeof(DialogueRunner))]
    public class NudgeDialogueRunner : MonoBehaviour
    {
		#region Fields

		private DialogueRunner m_dialogueRunner;

		#endregion

		#region Properties

		public DialogueRunner dialogueRunner => m_dialogueRunner;

		#endregion

		#region Methods

		private void Awake()
		{
			m_dialogueRunner = GetComponent<DialogueRunner>();
		}

		private void OnEnable()
		{
			NudgeManager.Instance.Register(this);
		}

		private void OnDisable()
		{
			NudgeManager.Instance.Unregister(this);
		}

		#endregion
	}
}