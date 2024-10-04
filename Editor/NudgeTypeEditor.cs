using UnityEngine;
using UnityEditor;
using ToolkitEngine.Dialogue;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(NudgeType))]
    public class NudgeTypeEditor : BaseToolkitEditor, INestableEditor
    {
		#region Fields

		protected SerializedProperty m_delayTime;
		protected SerializedProperty m_minDelayTime;
		protected SerializedProperty m_autoClear;

		#endregion

		#region Methods

		protected virtual void OnEnable()
		{
			m_delayTime = serializedObject.FindProperty(nameof(m_delayTime));
			m_minDelayTime = serializedObject.FindProperty(nameof(m_minDelayTime));
			m_autoClear = serializedObject.FindProperty(nameof(m_autoClear));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_delayTime);
			EditorGUILayout.PropertyField(m_minDelayTime);
			EditorGUILayout.PropertyField(m_autoClear);
		}

		public void OnNestedGUI(ref Rect position)
		{
			EditorGUIRectLayout.PropertyField(ref position, m_delayTime);
			EditorGUIRectLayout.PropertyField(ref position, m_minDelayTime);
			EditorGUIRectLayout.PropertyField(ref position, m_autoClear);
		}

		public float GetNestedHeight()
		{
			return EditorGUI.GetPropertyHeight(m_delayTime)
				+ EditorGUI.GetPropertyHeight(m_delayTime)
				+ EditorGUI.GetPropertyHeight(m_delayTime)
				+ (EditorGUIUtility.standardVerticalSpacing * 3f);
		}

		#endregion
	}
}