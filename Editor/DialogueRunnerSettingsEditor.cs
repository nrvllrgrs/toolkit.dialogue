using UnityEditor;
using ToolkitEngine.Dialogue;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(DialogueRunnerSettings))]
    public class DialogueRunnerSettingsEditor : BaseToolkitEditor
    {
		#region Fields

		protected SerializedProperty m_registration;
		protected SerializedProperty m_dialogueCategory;
		protected SerializedProperty m_dialogueType;
		protected SerializedProperty m_variableStorage;
		protected SerializedProperty m_dialogueViews;

		#endregion

		#region Methods

		private void OnEnable()
		{
			m_registration = serializedObject.FindProperty(nameof(m_registration));
			m_dialogueCategory = serializedObject.FindProperty(nameof(m_dialogueCategory));
			m_dialogueType = serializedObject.FindProperty(nameof(m_dialogueType));
			m_variableStorage = serializedObject.FindProperty(nameof(m_variableStorage));
			m_dialogueViews = serializedObject.FindProperty(nameof(m_dialogueViews));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_registration);

			++EditorGUI.indentLevel;
			switch ((DialogueRunnerSettings.RegistrationMode)m_registration.intValue)
			{
				case DialogueRunnerSettings.RegistrationMode.Category:
					EditorGUILayout.PropertyField(m_dialogueCategory);
					break;

				case DialogueRunnerSettings.RegistrationMode.Type:
					EditorGUILayout.PropertyField(m_dialogueType);
					break;
			}
			--EditorGUI.indentLevel;

			EditorGUILayout.PropertyField(m_variableStorage);
			EditorGUILayout.PropertyField(m_dialogueViews);
		}

		#endregion
	}
}