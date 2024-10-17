using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	public class PlayDialogue : BasePlayDialogueUnit
    {
		#region Methods

		protected override ControlOutput Trigger(Flow flow)
		{
			DialogueManager.CastInstance.Play(
				flow.GetValue<DialogueType>(dialogueType),
				flow.GetValue<YarnProject>(yarnProject),
				flow.GetValue<string>(startNode));

			return exit;
		}

		#endregion
	}
}