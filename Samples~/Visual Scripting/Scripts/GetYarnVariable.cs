using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Dialogue")]
	public class GetYarnVariable : Unit
	{
		#region Enumerators

		public enum VariableType
		{
			Boolean,
			Float,
			Integer,
		}

		#endregion

		#region Fields

		[DoNotSerialize, UnitHeaderInspectable]
		public VariableType variableType { get; set; }

		[DoNotSerialize, PortLabelHidden]
		public ControlInput enter { get; private set; }

		[DoNotSerialize, PortLabelHidden]
		public ValueInput dialogueRunner;

		[DoNotSerialize, PortLabelHidden]
		public ValueInput variableName;

		[DoNotSerialize, PortLabelHidden]
		public ControlOutput exit { get; private set; }

		[DoNotSerialize]
		public ValueOutput contains;

		[DoNotSerialize, PortLabelHidden]
		public ValueOutput asBool;

		[DoNotSerialize, PortLabelHidden]
		public ValueOutput asFloat;

		[DoNotSerialize, PortLabelHidden]
		public ValueOutput asInt;

		private bool m_contains;
		private bool m_asBool;
		private float m_asFloat;
		private int m_asInt;

		#endregion

		#region Methods

		protected override void Definition()
		{
			enter = ControlInput(nameof(enter), Trigger);
			dialogueRunner = ValueInput<DialogueRunner>(nameof(dialogueRunner), null);
			variableName = ValueInput(nameof(variableName), string.Empty);

			exit = ControlOutput(nameof(exit));
			Succession(enter, exit);

			contains = ValueOutput(nameof(contains), (x) => m_contains);

			switch (variableType)
			{
				case VariableType.Boolean:
					asBool = ValueOutput(nameof(asBool), (x) => m_asBool);
					break;

				case VariableType.Float:
					asFloat = ValueOutput(nameof(asFloat), (x) => m_asFloat);
					break;

				case VariableType.Integer:
					asInt = ValueOutput(nameof(asInt), (x) => m_asInt);
					break;
			}
		}

		private ControlOutput Trigger(Flow flow)
		{
			var variableStorage = flow.GetValue<DialogueRunner>(dialogueRunner)?.VariableStorage;
			var variableName = flow.GetValue<string>(this.variableName);

			m_contains = false;

			if (variableStorage != null && !string.IsNullOrWhiteSpace(variableName))
			{
				switch (variableType)
				{
					case VariableType.Boolean:
						if (variableStorage.TryGetValue(variableName, out bool b))
						{
							m_contains = true;
							m_asBool = b;
						}
						break;

					case VariableType.Float:
						if (variableStorage.TryGetValue(variableName, out float f))
						{
							m_contains = true;
							m_asFloat = f;
						}
						break;

					case VariableType.Integer:
						if (variableStorage.TryGetValue(variableName, out int i))
						{
							m_contains = true;
							m_asInt = i;
						}
						break;
				}
			}

			return exit;
		}

		#endregion
	}
}