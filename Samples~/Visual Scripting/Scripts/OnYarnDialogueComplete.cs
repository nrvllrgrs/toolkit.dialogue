using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue")]
	[UnitTitle("On Dialogue Complete"), UnitSurtitle("Dialogue Runner")]
	public class OnYarnDialogueComplete : BaseEventUnit<EmptyEventArgs>
	{
		protected override bool showEventArgs => false;
		public override Type MessageListenerType => typeof(OnYarnDialogueCompleteMessageListener);
	}
}