using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public static class DialogueRunnerExt
	{
		public static bool NodeExists(this DialogueRunner dialogueRunner, string nodeName)
		{
			if (dialogueRunner.YarnProject == null)
				return false;

			return dialogueRunner.YarnProject.NodeExists(nodeName);
		}
	}
}