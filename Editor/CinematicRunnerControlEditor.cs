using ToolkitEngine.Dialogue;
using UnityEditor;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(CinematicRunnerControl))]
	public class CinematicRunnerControlEditor : DialogueRunnerControlEditor
    {
		#region Fields

		protected SerializedProperty m_signal;
		protected SerializedProperty m_onCinematicSkipped;

		#endregion

		#region Methods

		protected override void OnEnable()
		{
			base.OnEnable();
			m_signal = serializedObject.FindProperty(nameof(m_signal));
			m_onCinematicSkipped = serializedObject.FindProperty(nameof(m_onCinematicSkipped));
		}

		protected override void DrawProperties()
		{
			EditorGUI.BeginDisabledGroup(true);
			m_dialogueType.objectReferenceValue = CinematicManager.CastInstance.Config?.dialogueType;
			EditorGUILayout.PropertyField(m_dialogueType);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.PropertyField(m_signal);
			EditorGUILayout.PropertyField(m_startNode);
			EditorGUILayout.PropertyField(m_playOnStart);

			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_replicateSettings);
			EditorGUILayout.PropertyField(m_appendDialogueViews);
		}

		protected override void DrawNestedEvents()
		{
			EditorGUILayout.PropertyField(m_onCinematicSkipped);
		}

		#endregion
	}
}