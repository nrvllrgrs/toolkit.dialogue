using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitTitle("On Node Started")]
    public class OnNodeStarted : BaseFilteredDialogueEventUnit
	{
		public override Type MessageListenerType => typeof(OnNodeStartedMessageListener);

		protected override void StartListeningToManager()
		{
			DialogueManager.CastInstance.NodeStarted += InvokeTrigger;
		}

		protected override void StopListeningToManager()
		{
			DialogueManager.CastInstance.NodeStarted -= InvokeTrigger;
		}

		protected override string GetFilterValue(DialogueEventArgs args) => args.nodeName;
	}
}