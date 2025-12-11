using System;
using System.Linq;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public static class DialogueCommands
    {
		#region Methods

		[YarnCommand("runDialogue")]
		public static void RunDialogue(string dialogueType, string yarnProject, string startNode)
		{
			var type = DialogueManager.CastInstance.GetDialogueTypes()
				.FirstOrDefault(x => string.Equals(x.name, dialogueType, StringComparison.OrdinalIgnoreCase));
			if (type == null)
				return;

			var project = DialogueManager.CastInstance.GetYarnProjects()
				.FirstOrDefault(x => string.Equals(x.name, yarnProject, StringComparison.OrdinalIgnoreCase));
			if (project == null)
				return;

			DialogueManager.CastInstance.Play(type, project, startNode);
		}

		#endregion
	}
}
