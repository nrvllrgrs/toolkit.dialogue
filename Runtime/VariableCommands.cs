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

		private DialogueRunner m_dialogueRunner;

		#endregion

		#region Properties

		public IVariableStorage variableStorage => m_dialogueRunner.VariableStorage;

		#endregion

		#region Methods

		private void Awake()
		{
			m_dialogueRunner = GetComponent<DialogueRunner>();
			m_dialogueRunner.AddCommandHandler<string>("waitWhile", WaitWhileVariable);
			m_dialogueRunner.AddCommandHandler<string>("waitUntil", WaitUntilVariable);
			m_dialogueRunner.AddCommandHandler<string, int>("increment", Increment);
			m_dialogueRunner.AddCommandHandler<string, int>("decrement", Decrement);
			m_dialogueRunner.AddCommandHandler<string, int, int>("incrementAndWrap", IncrementAndWrap);
			m_dialogueRunner.AddCommandHandler<string, int, int>("decrementAndWrap", DecrementAndWrap);
		}

		public IEnumerator WaitWhileVariable(string variableName)
		{
			if (!variableStorage.TryGetValue(variableName, out bool value) || !value)
				yield break;

			yield return new WaitWhile(() =>
			{
				return variableStorage.TryGetValue(variableName, out bool value) && value;
			});
		}

		public IEnumerator WaitUntilVariable(string variableName)
		{
			if (!variableStorage.TryGetValue(variableName, out bool value) || value)
				yield break;

			yield return new WaitUntil(() =>
			{
				return variableStorage.TryGetValue(variableName, out bool value) && value;
			});
		}

		public void Increment(string variableName, int delta = 1)
		{
			float value;
			if (!variableStorage.TryGetValue(variableName, out value))
			{
				value = 0f;
			}

			variableStorage.SetValue(variableName, value + delta);
		}

		public void Decrement(string variableName, int delta = 1)
		{
			Increment(variableName, -delta);
		}

		public void IncrementAndWrap(string variableName, int length, int delta = 1)
		{
			float value;
			if (!variableStorage.TryGetValue(variableName, out value))
			{
				value = 0f;
			}

			variableStorage.SetValue(variableName, (((value + delta) % length) + length) % length);
		}

		public void DecrementAndWrap(string variableName, int length, int delta = 1)
		{
			IncrementAndWrap(variableName, length, -delta);
		}

		#endregion
	}
}