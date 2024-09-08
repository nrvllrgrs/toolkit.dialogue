using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitTitle("On Node Start"), UnitSurtitle("Dialogue Runner")]
    public class OnYarnNodeStart : BaseYarnEventUnit
	{
		public override Type MessageListenerType => typeof(OnYarnNodeStartMessageListener);
	}
}