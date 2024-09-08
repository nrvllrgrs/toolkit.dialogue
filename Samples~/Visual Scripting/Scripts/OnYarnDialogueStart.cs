using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue")]
	[UnitTitle("On Dialogue Start"), UnitSurtitle("Dialogue Runner")]
	public class OnYarnDialogueStart : BaseEventUnit<EmptyEventArgs>
	{
		protected override bool showEventArgs => false;
		public override Type MessageListenerType => typeof(OnYarnDialogueStartMessageListener);
	}
}