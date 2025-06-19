using System.Collections.Generic;
using ToolkitEngine.Dialogue.SaveManagement;
using ToolkitEngine.SaveManagement;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue.SaveManagement
{
	[CustomEditor(typeof(DialogueVariableStorage))]
	public class DialogueVariableStorageEditor : BaseToolkitEditor
	{
		#region Fields

		private DialogueVariableStorage m_variableStorage;
		private DialogueRunner m_dialogueRunner;

		private int m_selectedIndex = 0;

		protected SerializedProperty m_boolVariables;
		protected SerializedProperty m_floatVariables;
		protected SerializedProperty m_stringVariables;

		#endregion

		#region Methods

		private void OnEnable()
		{
			m_variableStorage = target as DialogueVariableStorage;
			m_dialogueRunner = m_variableStorage.GetComponent<DialogueRunner>();

			m_boolVariables = serializedObject.FindProperty(nameof(m_boolVariables));
			m_floatVariables = serializedObject.FindProperty(nameof(m_floatVariables));
			m_stringVariables = serializedObject.FindProperty(nameof(m_stringVariables));
		}

		protected override void DrawProperties()
		{
			base.DrawProperties();

			List<string> dialogueVariables = new();
			if (m_dialogueRunner.yarnProject != null)
			{
				foreach (var p in m_dialogueRunner.yarnProject.InitialValues)
				{
					dialogueVariables.Add($"{p.Value.GetType().Name}/{p.Key}");
				}
				dialogueVariables.Sort();
			}

			EditorGUILayout.BeginHorizontal();
			{
				m_selectedIndex = EditorGUILayout.Popup("Bind Variable", m_selectedIndex, dialogueVariables.ToArray());

				if (GUILayout.Button("+", GUILayout.Width(EditorGUIUtility.singleLineHeight + 2)))
				{
					string variableName = dialogueVariables[m_selectedIndex];

					int breakIndex = variableName.IndexOf("/");
					string variableType = variableName.Substring(0, breakIndex);
					variableName = variableName.Substring(breakIndex + 1);

					if (m_variableStorage.ContainsKey(variableName))
					{
						EditorUtility.DisplayDialog(
							"Variable Mapping",
							$"Variable {variableName} is already mapped.",
							"OK");
					}
					else
					{
						switch (variableType)
						{
							case "Boolean":
								m_variableStorage.boolVariables.Add(variableName, new SaveBool());
								break;

							case "Single":
								m_variableStorage.floatVariables.Add(variableName, new SaveFloat());
								break;

							case "String":
								m_variableStorage.stringVariables.Add(variableName, new SaveString());
								break;
						}
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			int count = m_variableStorage.boolVariables.Count
				+ m_variableStorage.floatVariables.Count
				+ m_variableStorage.stringVariables.Count;

			if (count > 0)
			{
				m_boolVariables.isExpanded = EditorGUILayout.Foldout(m_boolVariables.isExpanded, "Variables");
				if (m_boolVariables.isExpanded)
				{
					++EditorGUI.indentLevel;
					{
						DrawVariables(m_boolVariables);
						DrawVariables(m_floatVariables);
						DrawVariables(m_stringVariables);
					}
					--EditorGUI.indentLevel;
				}
			}
		}

		private void DrawVariables(SerializedProperty property)
		{
			var keysProp = property.FindPropertyRelative("keys");
			var valuesProp = property.FindPropertyRelative("values");

			for (int i = 0; i < keysProp.arraySize; ++i)
			{
				EditorGUILayout.BeginHorizontal();
				{
					string key = keysProp.GetArrayElementAtIndex(i).stringValue;
					EditorGUILayout.PropertyField(
						valuesProp.GetArrayElementAtIndex(i),
						new GUIContent(key));

					var iconContent = EditorGUIUtility.IconContent("Close");
					iconContent.tooltip = "Remove Item";

					if (GUILayout.Button(iconContent, GUILayout.Width(EditorGUIUtility.singleLineHeight + 2)))
					{
						m_variableStorage.Remove(key);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		#endregion
	}
}