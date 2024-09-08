using UnityEditor;
using UnityEngine;
using ToolkitEngine.Dialogue;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(DialogueType))]
	public class DialogueTypeEditor : BaseToolkitEditor, INestableEditor
    {
		#region Fields

		protected SerializedProperty m_interruptPriority;
		protected SerializedProperty m_enqueueIfBlocked;
		protected SerializedProperty m_autoClearQueue;
		protected SerializedProperty m_skippable;

		#endregion

		#region Methods

		protected virtual void OnEnable()
		{
			m_interruptPriority = serializedObject.FindProperty(nameof(m_interruptPriority));
			m_enqueueIfBlocked = serializedObject.FindProperty(nameof(m_enqueueIfBlocked));
			m_autoClearQueue = serializedObject.FindProperty(nameof(m_autoClearQueue));
			m_skippable = serializedObject.FindProperty(nameof(m_skippable));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_interruptPriority);
			EditorGUILayout.PropertyField(m_enqueueIfBlocked);
			EditorGUILayout.PropertyField(m_autoClearQueue);
			EditorGUILayout.PropertyField(m_skippable);
		}

		public void OnNestedGUI(ref Rect position)
		{
			EditorGUIRectLayout.PropertyField(ref position, m_interruptPriority);
			EditorGUIRectLayout.PropertyField(ref position, m_enqueueIfBlocked);
			EditorGUIRectLayout.PropertyField(ref position, m_autoClearQueue);
			EditorGUIRectLayout.PropertyField(ref position, m_skippable);
		}

		public float GetNestedHeight()
		{
			return EditorGUI.GetPropertyHeight(m_interruptPriority)
				+ EditorGUI.GetPropertyHeight(m_enqueueIfBlocked)
				+ EditorGUI.GetPropertyHeight(m_autoClearQueue)
				+ EditorGUI.GetPropertyHeight(m_skippable)
				+ (EditorGUIUtility.standardVerticalSpacing * 4f);
		}

		#endregion
	}
}