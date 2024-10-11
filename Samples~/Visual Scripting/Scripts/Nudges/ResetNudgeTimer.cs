using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Dialogue/Nudge")]
	[UnitTitle("Reset Nudge Timer")]
	public class ResetNudgeTimer : Unit
	{
		#region Fields

		[DoNotSerialize, PortLabelHidden]
		public ControlInput enter;

		[DoNotSerialize, PortLabelHidden]
		public ControlOutput exit;

		#endregion

		#region Methods

		protected override void Definition()
		{
			enter = ControlInput(nameof(enter), Enter);
			exit = ControlOutput(nameof(exit));
			Succession(enter, exit);
		}

		private ControlOutput Enter(Flow flow)
		{
			NudgeManager.CastInstance.ResetTimer();
			return exit;
		}

		#endregion
	}
}