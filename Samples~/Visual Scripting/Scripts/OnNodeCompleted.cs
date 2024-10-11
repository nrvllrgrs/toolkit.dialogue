using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitTitle("On Node Completed")]
	public class OnNodeCompleted : BaseFilteredDialogueEventUnit
	{
		public override Type MessageListenerType => typeof(OnNodeCompletedMessageListener);

		protected override void StartListeningToManager()
		{
			DialogueManager.CastInstance.NodeCompleted += InvokeTrigger;
		}

		protected override void StopListeningToManager()
		{
			DialogueManager.CastInstance.NodeCompleted -= InvokeTrigger;
		}

		protected override string GetFilterValue(DialogueEventArgs args) => args.nodeName;
	}
}