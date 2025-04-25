using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Dialogue")]
	public class SetYarnVariable : Unit
	{
		#region Fields

		[DoNotSerialize, UnitHeaderInspectable]
		public YarnVariableType variableType { get; set; }

		[DoNotSerialize, PortLabelHidden]
		public ControlInput enter { get; private set; }

		[DoNotSerialize, PortLabelHidden]
		public ValueInput dialogueRunner;

		[DoNotSerialize, PortLabelHidden]
		public ValueInput variableName;

		[DoNotSerialize, PortLabelHidden]
		public ControlOutput exit { get; private set; }

		[DoNotSerialize, PortLabelHidden]
		public ValueInput asBool;

		[DoNotSerialize, PortLabelHidden]
		public ValueInput asFloat;

		[DoNotSerialize, PortLabelHidden]
		public ValueInput asInt;

		#endregion

		#region Methods

		protected override void Definition()
		{
			enter = ControlInput(nameof(enter), Trigger);
			dialogueRunner = ValueInput<DialogueRunner>(nameof(dialogueRunner), null);
			variableName = ValueInput(nameof(variableName), string.Empty);

			exit = ControlOutput(nameof(exit));
			Succession(enter, exit);

			switch (variableType)
			{
				case YarnVariableType.Boolean:
					asBool = ValueInput(nameof(asBool), false);
					break;

				case YarnVariableType.Float:
					asFloat = ValueInput(nameof(asFloat), 0f);
					break;

				case YarnVariableType.Integer:
					asInt = ValueInput(nameof(asInt), 0);
					break;
			}
		}

		private ControlOutput Trigger(Flow flow)
		{
			var variableStorage = flow.GetValue<DialogueRunner>(dialogueRunner)?.VariableStorage;
			var variableName = flow.GetValue<string>(this.variableName);

			if (variableStorage != null && !string.IsNullOrWhiteSpace(variableName))
			{
				switch (variableType)
				{
					case YarnVariableType.Boolean:
						variableStorage.SetValue(variableName, flow.GetValue<bool>(asBool));
						break;

					case YarnVariableType.Float:
						variableStorage.SetValue(variableName, flow.GetValue<float>(asFloat));
						break;

					case YarnVariableType.Integer:
						variableStorage.SetValue(variableName, flow.GetValue<int>(asInt));
						break;
				}
			}

			return exit;
		}

		#endregion
	}
}