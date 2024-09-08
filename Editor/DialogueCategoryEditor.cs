using UnityEditor;
using UnityEngine;
using ToolkitEngine.Dialogue;

namespace ToolkitEditor.Dialogue
{
	[CustomEditor(typeof(DialogueCategory))]
    public class DialogueCategoryEditor : BaseToolkitEditor, INestableEditor
    {
		#region Fields

		protected SerializedProperty m_priorities;
		protected SerializedProperty m_infiniteSimulatenous;
		protected SerializedProperty m_maxSimultaneous;
		protected SerializedProperty m_interruptPriority;
		protected SerializedProperty m_queueable;
		protected SerializedProperty m_queuePriority;
		protected SerializedProperty m_timeToForget;

		#endregion

		#region Methods

		private void OnEnable()
		{
			m_priorities = serializedObject.FindProperty(nameof(m_priorities));
			m_infiniteSimulatenous = serializedObject.FindProperty(nameof(m_infiniteSimulatenous));
			m_maxSimultaneous = serializedObject.FindProperty(nameof(m_maxSimultaneous));
			m_interruptPriority = serializedObject.FindProperty(nameof(m_interruptPriority));
			m_queueable = serializedObject.FindProperty(nameof(m_queueable));
			m_queuePriority = serializedObject.FindProperty(nameof(m_queuePriority));
			m_timeToForget = serializedObject.FindProperty(nameof(m_timeToForget));
		}

		protected override void DrawProperties()
		{
			base.DrawProperties();

			EditorGUILayout.PropertyField(m_priorities);

			EditorGUILayout.PropertyField(m_infiniteSimulatenous);
			if (!m_infiniteSimulatenous.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_maxSimultaneous);

				if (m_maxSimultaneous.intValue > 1)
				{
					EditorGUILayout.PropertyField(m_interruptPriority);
				}
				--EditorGUI.indentLevel;
			}

			EditorGUILayout.PropertyField(m_queueable);
			if (m_queueable.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_queuePriority);
				EditorGUILayout.PropertyField(m_timeToForget);
				--EditorGUI.indentLevel;
			}
		}

		public virtual void OnNestedGUI(ref Rect position)
		{
			EditorGUIRectLayout.PropertyField(ref position, m_priorities);

			EditorGUIRectLayout.PropertyField(ref position, m_infiniteSimulatenous);
			if (!m_infiniteSimulatenous.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUIRectLayout.PropertyField(ref position, m_maxSimultaneous);

				if (m_maxSimultaneous.intValue > 1)
				{
					EditorGUIRectLayout.PropertyField(ref position, m_interruptPriority);
				}
				--EditorGUI.indentLevel;
			}

			EditorGUIRectLayout.PropertyField(ref position, m_queueable);
			if (m_queueable.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUIRectLayout.PropertyField(ref position, m_queuePriority);
				EditorGUIRectLayout.PropertyField(ref position, m_timeToForget);
				--EditorGUI.indentLevel;
			}
		}

		public virtual float GetNestedHeight()
		{
			float height = EditorGUI.GetPropertyHeight(m_priorities)
				+ EditorGUI.GetPropertyHeight(m_infiniteSimulatenous)
				+ EditorGUI.GetPropertyHeight(m_queueable)
				+ (EditorGUIUtility.standardVerticalSpacing * 3f);

			if (!m_infiniteSimulatenous.boolValue)
			{
				height += EditorGUI.GetPropertyHeight(m_maxSimultaneous)
					+ EditorGUIUtility.standardVerticalSpacing;

				if (m_maxSimultaneous.intValue > 1)
				{
					height += EditorGUI.GetPropertyHeight(m_interruptPriority)
						+ EditorGUIUtility.standardVerticalSpacing;
				}
			}

			if (m_queueable.boolValue)
			{
				height += EditorGUI.GetPropertyHeight(m_queuePriority)
					+ EditorGUI.GetPropertyHeight(m_timeToForget)
					+ (EditorGUIUtility.standardVerticalSpacing * 2f);
			}

			return height;
		}

		#endregion
	}
}