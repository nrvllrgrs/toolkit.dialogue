using System.Collections;
using UnityEngine;
using Yarn;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
    [RequireComponent(typeof(DialogueRunner))]
    public class VariableCommands : MonoBehaviour
    {
		#region Fields

		private IVariableStorage m_variableStorage;

		#endregion

		#region Methods

		private void Awake()
		{
			m_variableStorage = GetComponent<DialogueRunner>()?.VariableStorage;
		}

		[YarnCommand("waitWhile")]
		public IEnumerator WaitWhileVariable(string variableName)
		{
			if (!m_variableStorage.TryGetValue(variableName, out bool value) || !value)
				yield break;

			yield return new WaitWhile(() =>
			{
				return m_variableStorage.TryGetValue(variableName, out bool value) && value;
			});
		}

		[YarnCommand("waitUntil")]
		public IEnumerator WaitUntilVariable(string variableName)
		{
			if (!m_variableStorage.TryGetValue(variableName, out bool value) || value)
				yield break;

			yield return new WaitUntil(() =>
			{
				return m_variableStorage.TryGetValue(variableName, out bool value) && value;
			});
		}

		#endregion
	}
}