using ToolkitEngine.Dialogue;
using UnityEditor;
using UnityEngine;

namespace ToolkitEditor.Dialogue
{
	[CustomPropertyDrawer(typeof(DialogueRegistration))]
	public class DialogueRegistrationDrawer : PropertyDrawer
	{
		#region Fields

		protected SerializedProperty m_mode;
		protected SerializedProperty m_dialogueCategory;
		protected SerializedProperty m_dialogueType;

		#endregion

		#region Methods

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Initialize(property);
			EditorGUIRectLayout.PropertyField(ref position, m_mode);

			++EditorGUI.indentLevel;
			switch ((DialogueRegistration.Mode)m_mode.intValue)
			{
				case DialogueRegistration.Mode.Category:
					EditorGUIRectLayout.PropertyField(ref position, m_dialogueCategory);
					break;

				case DialogueRegistration.Mode.Type:
					EditorGUIRectLayout.PropertyField(ref position, m_dialogueType);
					break;
			}
			--EditorGUI.indentLevel;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Initialize(property);

			float height = EditorGUI.GetPropertyHeight(m_mode)
				+ EditorGUIUtility.standardVerticalSpacing * 2f;

			switch ((DialogueRegistration.Mode)m_mode.intValue)
			{
				case DialogueRegistration.Mode.Category:
					height += EditorGUI.GetPropertyHeight(m_dialogueCategory);
					break;

				case DialogueRegistration.Mode.Type:
					height += EditorGUI.GetPropertyHeight(m_dialogueType);
					break;
			}

			return height;
		}

		private void Initialize(SerializedProperty property)
		{
			if (m_mode == null)
			{
				m_mode = property.FindPropertyRelative(nameof(m_mode));
				m_dialogueCategory = property.FindPropertyRelative(nameof(m_dialogueCategory));
				m_dialogueType = property.FindPropertyRelative(nameof(m_dialogueType));
			}
		}

		#endregion
	}
}