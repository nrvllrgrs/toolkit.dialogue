using Yarn;

namespace ToolkitEditor.Dialogue
{
	public class EmptyVariableStorage : IVariableStorage
	{
		#region Properties

		public Program Program { get; set; }
		public ISmartVariableEvaluator SmartVariableEvaluator { get; set; }

		#endregion

		#region Methods

		public void Clear()
		{ }

		public VariableKind GetVariableKind(string name)
		{
			return VariableKind.Unknown;
		}

		public void SetValue(string variableName, string stringValue)
		{ }

		public void SetValue(string variableName, float floatValue)
		{ }

		public void SetValue(string variableName, bool boolValue)
		{ }

		public bool TryGetValue<T>(string variableName, out T result)
		{
			result = default(T);
			return false;
		}

		#endregion
	}
}
