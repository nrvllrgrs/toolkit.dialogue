using ToolkitEngine.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	public static class VisualScriptingCommands
    {
        [YarnCommand("runGraph")]
        public static void RunGraph(string key)
        {
            VisualScriptingManager.CastInstance.Trigger(VisualScriptingManager.GetEventHookName<OnRunGraph>(key));
        }
    }
}