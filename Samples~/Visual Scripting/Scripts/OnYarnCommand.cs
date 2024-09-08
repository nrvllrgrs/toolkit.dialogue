using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue")]
	[UnitTitle("On Command"), UnitSurtitle("Dialogue Runner")]
	public class OnYarnCommand : BaseEventUnit<string>
	{
		public override Type MessageListenerType => typeof(OnYarnCommandMessageListener);
	}
}