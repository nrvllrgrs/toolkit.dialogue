using UnityEngine;
using Yarn.Unity;

using ToolkitEngine.SaveManagement;

namespace ToolkitEngine.Dialogue.SaveManagement
{
	[RequireComponent(typeof(DialogueRunner))]
    public class DialogueVariableStorage : MonoBehaviour, ISaveable
    {
		#region Fields

		[SerializeField, HideInInspector]
		private BoolVariableMap m_boolVariables = new();

		[SerializeField, HideInInspector]
		private FloatVariableMap m_floatVariables = new();

		[SerializeField, HideInInspector]
		private StringVariableMap m_stringVariables = new();

		private DialogueRunner m_dialogueRunner;

		#endregion

		#region Properties

		internal BoolVariableMap boolVariables => m_boolVariables;
		internal FloatVariableMap floatVariables => m_floatVariables;
		internal StringVariableMap stringVariables => m_stringVariables;

		#endregion

		#region Methods

		private void Awake()
		{
			m_dialogueRunner = GetComponent<DialogueRunner>();
			m_dialogueRunner.onDialogueStart.AddListener(DialogueRunner_DialogueStarted);
			m_dialogueRunner.onDialogueComplete.AddListener(DialogueRunner_DialogeCompleted);
		}

		private void OnDestroy()
		{
			m_dialogueRunner.onDialogueStart.RemoveListener(DialogueRunner_DialogueStarted);
			m_dialogueRunner.onDialogueComplete.RemoveListener(DialogueRunner_DialogeCompleted);
		}

		private void DialogueRunner_DialogueStarted()
		{
			Load();
		}

		[ContextMenu("Load")]
		public void Load()
		{
			if (m_dialogueRunner.VariableStorage == null)
				return;

			Load(m_boolVariables, m_dialogueRunner.VariableStorage.SetValue);
			Load(m_floatVariables, m_dialogueRunner.VariableStorage.SetValue);
			Load(m_stringVariables, m_dialogueRunner.VariableStorage.SetValue);
		}

		private void Load<T, K>(VariableMap<T, K> map, System.Action<string, T> saveValue)
			where K : SaveVariable<T>
		{
			foreach (var p in map)
			{
				saveValue(p.Key, p.Value.value);
			}
		}

		private void DialogueRunner_DialogeCompleted()
		{
			Save();
		}

		[ContextMenu("Save")]
		public void Save()
		{
			if (m_dialogueRunner.VariableStorage == null)
				return;

			Save(m_boolVariables);
			Save(m_floatVariables);
			Save(m_stringVariables);
		}

		private void Save<T, K>(VariableMap<T, K> map)
			where K : SaveVariable<T>
		{
			foreach (var p in map)
			{
				if (m_dialogueRunner.VariableStorage.TryGetValue<T>(p.Key, out var value))
				{
					p.Value.value = value;
				}
			}
		}

		public bool ContainsKey(string name)
		{
			return m_boolVariables.ContainsKey(name)
				|| m_floatVariables.ContainsKey(name)
				|| m_stringVariables.ContainsKey(name);
		}

		#endregion

		#region Structures

		internal abstract class VariableMap<T, K> : SerializableDictionary<string, K>
			where K : SaveVariable<T>
		{ }

		[System.Serializable]
		internal class BoolVariableMap : VariableMap<bool, SaveBool>
		{ }

		[System.Serializable]
		internal class FloatVariableMap : VariableMap<float, SaveFloat>
		{ }

		[System.Serializable]
		internal class StringVariableMap : VariableMap<string, SaveString>
		{ }

		#endregion
	}
}