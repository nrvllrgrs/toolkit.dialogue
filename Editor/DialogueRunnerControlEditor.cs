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
		protected SerializedProperty m_onDialogueStart;
		protected SerializedProperty m_onDialogueComplete;

		#endregion

		#region Methods

		private void OnEnable()
		{
			m_dialogueType = serializedObject.FindProperty(nameof(m_dialogueType));
			m_playOnStart = serializedObject.FindProperty(nameof (m_playOnStart));
			m_startNode = serializedObject.FindProperty(nameof(m_startNode));
			m_replicateSettings = serializedObject.FindProperty(nameof(m_replicateSettings));
			m_onDialogueStart = serializedObject.FindProperty(nameof (m_onDialogueStart));
			m_onDialogueComplete = serializedObject.FindProperty(nameof(m_onDialogueComplete));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_dialogueType);
			EditorGUILayout.PropertyField(m_startNode);
			EditorGUILayout.PropertyField(m_playOnStart);
			EditorGUILayout.PropertyField(m_replicateSettings);
		}

		protected override void DrawEvents()
		{
			if (EditorGUILayoutUtility.Foldout(m_onDialogueStart, "Events"))
			{
				EditorGUILayout.PropertyField(m_onDialogueStart);
				EditorGUILayout.PropertyField(m_onDialogueComplete);
			}
		}

		#endregion
	}
}