using ToolkitEngine.VisualScripting;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue")]
	public abstract class BaseDialogueEventUnit : FilteredTargetEventUnit<DialogueEventArgs, DialogueRunnerControl>
	{
		#region Methods

		protected override DialogueRunnerControl GetFilterValue(DialogueEventArgs args) => args.control;

		#endregion
	}
}