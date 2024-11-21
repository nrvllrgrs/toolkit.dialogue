using ToolkitEngine.VisualScripting;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Events/Dialogue/Cinematic")]
	[UnitTitle("On Skippable Changed")]
	public class OnSkippableChanged : ManagerEventUnit<bool>
	{
		#region Methods

		protected override void StartListeningToManager()
		{
			CinematicManager.CastInstance.SkippableChanged += InvokeTrigger;
		}

		protected override void StopListeningToManager()
		{
			CinematicManager.CastInstance.SkippableChanged -= InvokeTrigger;
		}

		#endregion
	}
}