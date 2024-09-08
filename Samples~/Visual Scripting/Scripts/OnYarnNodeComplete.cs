using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitTitle("On Node Complete"), UnitSurtitle("Dialogue Runner")]
	public class OnYarnNodeComplete : BaseYarnEventUnit
	{
		public override Type MessageListenerType => typeof(OnYarnNodeCompleteMessageListener);
	}
}