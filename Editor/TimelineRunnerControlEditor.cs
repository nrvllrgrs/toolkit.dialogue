using ToolkitEngine.Dialogue;
using UnityEditor;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(TimelineRunnerControl))]
	public class TimelineRunnerControlEditor : DialogueRunnerControlEditor
    {
		#region Fields

		protected SerializedProperty m_directors;
		protected SerializedProperty m_onSkipped;

		#endregion

		#region Methods

		protected override void OnEnable()
		{
			base.OnEnable();
			m_directors = serializedObject.FindProperty(nameof(m_directors));
			m_onSkipped = serializedObject.FindProperty(nameof(m_onSkipped));
		}

		protected override void DrawCustomProperties()
		{
			EditorGUILayout.PropertyField(m_directors);
		}

		protected override void DrawNestedEvents()
		{
			EditorGUILayout.PropertyField(m_onSkipped);
		}

		#endregion
	}
}