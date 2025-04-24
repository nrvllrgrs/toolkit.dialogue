using System.Collections.Generic;
using ToolkitEngine.Dialogue;
using Yarn;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue
{
	public static class YarnEditorUtil
    {
        public static IEnumerable<YarnProject> GetYarnProjects() => AssetUtil.GetAssetsOfType<YarnProject>();
        public static IEnumerable<DialogueSpeakerType> GetDialogueSpeakerTypes() => AssetUtil.GetAssetsOfType<DialogueSpeakerType>();

        public static Yarn.Dialogue GetDialogue(YarnProject project)
        {
            var dialogue = new Yarn.Dialogue(new EmptyVariableStorage());
            dialogue.SetProgram(project.Program);

            return dialogue;
		}
    }
}