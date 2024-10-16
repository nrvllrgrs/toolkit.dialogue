using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitTitle("On Dialogue Started")]
	public class OnDialogueStarted : BaseDialogueEventUnit
	{
		public override Type MessageListenerType => typeof(OnDialogueStartedMessageListener);

		protected override void StartListeningToManager()
		{
			DialogueManager.CastInstance.DialogueStarted += InvokeTrigger;
		}

		protected override void StopListeningToManager()
		{
			DialogueManager.CastInstance.DialogueStarted -= InvokeTrigger;
		}
	}
}