using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Dialogue")]
	public class EnqueueDialogue : Unit
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
		}

		private ControlOutput Trigger(Flow flow)
		{
			DialogueManager.CastInstance.Enqueue(
				flow.GetValue<DialogueType>(dialogueType),
				flow.GetValue<YarnProject>(yarnProject),
				flow.GetValue<string>(startNode));

			return exit;
		}

		#endregion
	}
}