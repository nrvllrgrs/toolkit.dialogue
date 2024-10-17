using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Dialogue")]
	public abstract class BasePlayDialogueUnit : Unit
	{
		#region Properties

		[DoNotSerialize, PortLabelHidden]
		public ControlInput enter { get; private set; }

		[DoNotSerialize, PortLabelHidden]
		public ControlOutput exit { get; private set; }

		[DoNotSerialize]
		public ValueInput dialogueType;

		[DoNotSerialize]
		public ValueInput yarnProject;

		[DoNotSerialize]
		public ValueInput startNode;

		[DoNotSerialize, PortLabelHidden]
		public ValueOutput runnerControl;

		#endregion

		#region Methods

		protected override void Definition()
		{
			enter = ControlInput(nameof(enter), Trigger);
			exit = ControlOutput(nameof(exit));
			Succession(enter, exit);

			dialogueType = ValueInput<DialogueType>(nameof(dialogueType), null);
			yarnProject = ValueInput<YarnProject>(nameof(yarnProject), null);
			startNode = ValueInput<string>(nameof(startNode), null);

			Requirement(dialogueType, enter);
			Requirement(yarnProject, enter);
			Requirement(startNode, enter);

			runnerControl = ValueOutput(nameof(runnerControl), GetDialogueRunnerControl);
		}

		protected abstract ControlOutput Trigger(Flow flow);

		private DialogueRunnerControl GetDialogueRunnerControl(Flow flow)
		{
			return DialogueManager.CastInstance.GetDialogueRunnerControl(
				flow.GetValue<DialogueType>(dialogueType),
				flow.GetValue<YarnProject>(yarnProject),
				flow.GetValue<string>(startNode));
		}

		#endregion
	}
}