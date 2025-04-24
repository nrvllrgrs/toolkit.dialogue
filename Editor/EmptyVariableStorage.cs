namespace ToolkitEditor.Dialogue
{
	public class EmptyVariableStorage : Yarn.IVariableStorage
	{
		public void Clear()
		{ }

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
	}
}
