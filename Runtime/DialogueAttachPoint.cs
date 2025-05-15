using UnityEngine;

namespace ToolkitEngine.Dialogue
{
    public class DialogueAttachPoint : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		private DialogueRegistration m_registration;

		[SerializeField]
		private AttachPoint m_attachPoint;

		#endregion

		#region Methods

		private void Awake()
		{
			m_attachPoint.Initalize(transform);
		}

		private void OnEnable()
		{
			DialogueManager.CastInstance.DialogueStarted += DialogueManager_DialogueStarted;
		}

		private void OnDisable()
		{
			if (DialogueManager.Exists)
			{
				DialogueManager.CastInstance.DialogueStarted -= DialogueManager_DialogueStarted;
			}
		}

		private void DialogueManager_DialogueStarted(object sender, DialogueEventArgs e)
		{
			// Not matching DialogueType, skip
			if (!m_registration.IsValid(e.type))
				return;

			DialogueManager.CastInstance.DialogueStarted -= DialogueManager_DialogueStarted;

			if (DialogueManager.CastInstance.TryGetDialogueRunnerSettings(m_registration, out var settings))
			{
				m_attachPoint.Attach(settings.transform);
			}
		}

		#endregion
	}
}