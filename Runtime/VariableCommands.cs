using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
			m_dialogueRunner.AddCommandHandler<string>("shuffle", Shuffle);
			m_dialogueRunner.AddFunction<string, string>("dequeue", Dequeue);
			m_dialogueRunner.AddFunction<string, float>("dequeueAsNumber", DequeueAsNumber);
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

		#region Array Methods

		public delegate string ArrayFunc(ref List<string> array);

		public void Shuffle(string variableName)
		{
			ModifyArray(variableName, Internal);

			string Internal(ref List<string> array)
			{
				array = array.Shuffle() as List<string>;
				return string.Empty;
			}
		}

		public string Dequeue(string variableName)
		{
			return ModifyArray(variableName, Internal);

			string Internal(ref List<string> array)
			{
				string result = array[0];
				array.RemoveAt(0);
				return result;
			}
		}

		public float DequeueAsNumber(string variableName)
		{
			return float.Parse(Dequeue(variableName));
		}

		private string ModifyArray(string variableName, ArrayFunc func)
		{
			string result = string.Empty;
			if (variableStorage.TryGetValue(variableName, out string value))
			{
				var split = value.Split(' ').ToList();
				result = func(ref split);
				value = string.Join(' ', split);
				variableStorage.SetValue(variableName, value);
			}

			return result;
		}

		#endregion
	}
}