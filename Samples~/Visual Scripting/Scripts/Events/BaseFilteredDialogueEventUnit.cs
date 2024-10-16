using ToolkitEngine.VisualScripting;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue")]
	public abstract class BaseFilteredDialogueEventUnit : FilteredTargetEventUnit<DialogueEventArgs, string>
    { }
}