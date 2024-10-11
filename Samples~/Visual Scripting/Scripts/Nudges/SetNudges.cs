using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Dialogue/Nudge")]
	[UnitTitle("Set Nudges")]
	public class SetNudges : Unit
	{
		#region Fields

		[DoNotSerialize, PortLabelHidden]
		public ControlInput enter;

		[DoNotSerialize, PortLabelHidden]
		public ControlOutput exit;

		[DoNotSerialize]
		public ValueInput nudgeType;

		[DoNotSerialize]
		public ValueInput project;

		[DoNotSerialize]
		public ValueInput startNode;

		[DoNotSerialize]
		public ValueInput playImmediately;

		#endregion

		#region Methods

		protected override void Definition()
		{
			enter = ControlInput(nameof(enter), Enter);
			exit = ControlOutput(nameof(exit));
			Succession(enter, exit);

			nudgeType = ValueInput<NudgeType>(nameof(nudgeType), null);
			project = ValueInput<YarnProject>(nameof(project), null);
			startNode = ValueInput(nameof(startNode), "Start");
			playImmediately = ValueInput(nameof(playImmediately), false);

			Requirement(nudgeType, enter);
			Requirement(project, enter);
		}

		private ControlOutput Enter(Flow flow)
		{
			NudgeManager.CastInstance.Set(
				flow.GetValue<NudgeType>(nudgeType),
				flow.GetValue<YarnProject>(project),
				flow.GetValue<string>(startNode),
				flow.GetValue<bool>(playImmediately));
			return exit;
		}

		#endregion
	}
}