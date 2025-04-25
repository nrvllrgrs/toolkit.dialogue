using UnityEditor;
using ToolkitEngine.Dialogue;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(DialogueRunnerControl))]
    public class DialogueRunnerControlEditor : BaseToolkitEditor
    {
		#region Fields

		protected DialogueRunner m_dialogueRunner;
		protected SerializedObject m_serializedDialogueRunner;
		protected SerializedProperty m_yarnProject;

		protected SerializedProperty m_dialogueType;
		protected SerializedProperty m_playOnStart;
		protected SerializedProperty m_startNode;
		protected SerializedProperty m_replicateSettings;
		protected SerializedProperty m_appendDialogueViews;
		protected SerializedProperty m_keepVariableStorage;

		protected SerializedProperty m_onDialogueStarted;
		protected SerializedProperty m_onDialogueCompleted;
		protected SerializedProperty m_onNodeStarted;
		protected SerializedProperty m_onNodeCompleted;
		protected SerializedProperty m_onCommand;

		#endregion

		#region Methods

		protected virtual void OnEnable()
		{
			m_dialogueRunner = (target as DialogueRunnerControl).GetComponent<DialogueRunner>();
			m_serializedDialogueRunner = new SerializedObject(m_dialogueRunner);

			// Need to make sure DialogueRunner does not start automatically
			m_dialogueRunner.startAutomatically = false;
			m_serializedDialogueRunner.ApplyModifiedProperties();

			m_yarnProject = m_serializedDialogueRunner.FindProperty("yarnProject");

			m_dialogueType = serializedObject.FindProperty(nameof(m_dialogueType));
			m_playOnStart = serializedObject.FindProperty(nameof (m_playOnStart));
			m_startNode = serializedObject.FindProperty(nameof(m_startNode));
			m_replicateSettings = serializedObject.FindProperty(nameof(m_replicateSettings));
			m_appendDialogueViews = serializedObject.FindProperty(nameof(m_appendDialogueViews));
			m_keepVariableStorage = serializedObject.FindProperty(nameof(m_keepVariableStorage));

			m_onDialogueStarted = serializedObject.FindProperty(nameof (m_onDialogueStarted));
			m_onDialogueCompleted = serializedObject.FindProperty(nameof(m_onDialogueCompleted));
			m_onNodeStarted = serializedObject.FindProperty(nameof(m_onNodeStarted));
			m_onNodeCompleted = serializedObject.FindProperty(nameof(m_onNodeCompleted));
			m_onCommand = serializedObject.FindProperty(nameof(m_onCommand));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_dialogueType);
			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.PropertyField(m_startNode);
			}
			if (EditorGUI.EndChangeCheck())
			{
				m_yarnProject.objectReferenceValue = (m_startNode.boxedValue as YarnNode)?.project;
				m_serializedDialogueRunner.ApplyModifiedProperties();
			}

			EditorGUILayout.PropertyField(m_playOnStart);

			EditorGUILayout.Separator();

			EditorGUI.BeginDisabledGroup(m_appendDialogueViews.boolValue
				|| m_keepVariableStorage.boolValue);
			{
				EditorGUILayout.PropertyField(m_replicateSettings);
			}
			EditorGUI.EndDisabledGroup();

			if (m_replicateSettings.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_appendDialogueViews);
				EditorGUILayout.PropertyField(m_keepVariableStorage);
				--EditorGUI.indentLevel;
			}
		}

		protected override void DrawEvents()
		{
			if (EditorGUILayoutUtility.Foldout(m_onDialogueStarted, "Events"))
			{
				EditorGUILayout.LabelField("Dialogue", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(m_onDialogueStarted);
				EditorGUILayout.PropertyField(m_onDialogueCompleted);

				EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(m_onNodeStarted);
				EditorGUILayout.PropertyField(m_onNodeCompleted);

				EditorGUILayout.LabelField("Command", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(m_onCommand);

				DrawNestedEvents();
			}
		}

		#endregion
	}
}