using UnityEditor;
using ToolkitEngine.Dialogue;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(DialogueRunnerControl))]
    public class DialogueRunnerControlEditor : BaseToolkitEditor
    {
		#region Fields

		protected SerializedProperty m_dialogueType;
		protected SerializedProperty m_playOnStart;
		protected SerializedProperty m_startNode;
		protected SerializedProperty m_replicateSettings;
		protected SerializedProperty m_appendDialogueViews;

		protected SerializedProperty m_onDialogueStarted;
		protected SerializedProperty m_onDialogueCompleted;
		protected SerializedProperty m_onNodeStarted;
		protected SerializedProperty m_onNodeCompleted;
		protected SerializedProperty m_onCommand;

		#endregion

		#region Methods

		protected virtual void OnEnable()
		{
			m_dialogueType = serializedObject.FindProperty(nameof(m_dialogueType));
			m_playOnStart = serializedObject.FindProperty(nameof (m_playOnStart));
			m_startNode = serializedObject.FindProperty(nameof(m_startNode));
			m_replicateSettings = serializedObject.FindProperty(nameof(m_replicateSettings));
			m_appendDialogueViews = serializedObject.FindProperty(nameof(m_appendDialogueViews));

			m_onDialogueStarted = serializedObject.FindProperty(nameof (m_onDialogueStarted));
			m_onDialogueCompleted = serializedObject.FindProperty(nameof(m_onDialogueCompleted));
			m_onNodeStarted = serializedObject.FindProperty(nameof(m_onNodeStarted));
			m_onNodeCompleted = serializedObject.FindProperty(nameof(m_onNodeCompleted));
			m_onCommand = serializedObject.FindProperty(nameof(m_onCommand));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_dialogueType);
			EditorGUILayout.PropertyField(m_startNode);
			EditorGUILayout.PropertyField(m_playOnStart);

			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_replicateSettings);
			EditorGUILayout.PropertyField(m_appendDialogueViews);
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