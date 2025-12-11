using UnityEditor;
using UnityEngine;
using ToolkitEngine.Dialogue;
using System;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue
{
	[CustomPropertyDrawer(typeof(YarnNode))]
    public class YarnNodeDrawer : PropertyDrawer
    {
		#region Fields

		protected SerializedProperty m_project;
		protected SerializedProperty m_name;

		#endregion

		#region Methods

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			m_project ??= property.FindPropertyRelative(nameof(m_project));
			m_name ??= property.FindPropertyRelative(nameof(m_name));

			EditorGUI.BeginProperty(position, GUIContent.none, property);
			{
				int selectedIndex = 0;
				EditorGUI.BeginChangeCheck();
				{
					if (!string.IsNullOrWhiteSpace(m_name.stringValue)
						&& m_project.objectReferenceValue != null)
					{
						var tuple = new Tuple<YarnProject, string>(m_project.objectReferenceValue as YarnProject, m_name.stringValue);
						if (YarnProjectPostprocessor.TryGetPath(tuple, out var selectedPath))
						{
							selectedIndex = YarnProjectPostprocessor.IndexOfPath(selectedPath);
						}
					}

					var tooltipRect = position;
					tooltipRect.x += EditorGUIUtility.labelWidth;
					tooltipRect.width -= EditorGUIUtility.labelWidth;

					selectedIndex = EditorGUIRectLayout.Popup(ref position, label.text, selectedIndex, YarnProjectPostprocessor.Paths);

					if (selectedIndex == 0 && !string.IsNullOrEmpty(m_name.stringValue))
					{
						++EditorGUI.indentLevel;
						m_name.stringValue = EditorGUIRectLayout.TextField(ref position, "Name", m_name.stringValue);
						--EditorGUI.indentLevel;
					}

					if (selectedIndex > 0 && YarnProjectPostprocessor.TryGetYarnProjectTuple(YarnProjectPostprocessor.Paths[selectedIndex], out var selectedTuple))
					{
						EditorGUI.LabelField(tooltipRect, new GUIContent(string.Empty, selectedTuple.Item2));
					}
				}
				if (EditorGUI.EndChangeCheck())
				{
					if (selectedIndex == 0)
					{
						m_project.objectReferenceValue = null;
						m_name.stringValue = string.Empty;
					}
					else if (YarnProjectPostprocessor.TryGetYarnProjectTuple(YarnProjectPostprocessor.Paths[selectedIndex], out var tuple))
					{
						m_project.objectReferenceValue = tuple.Item1;
						m_name.stringValue = tuple.Item2;
					}
				}
			}
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = base.GetPropertyHeight(property, label);
            if (m_name != null)
            {
				int selectedIndex = 0;
				if (!string.IsNullOrWhiteSpace(m_name.stringValue)
					&& m_project.objectReferenceValue != null)
				{
					var tuple = new Tuple<YarnProject, string>(m_project.objectReferenceValue as YarnProject, m_name.stringValue);
					if (YarnProjectPostprocessor.TryGetPath(tuple, out var selectedPath))
					{
						selectedIndex = YarnProjectPostprocessor.IndexOfPath(selectedPath);
					}
				}

				if (selectedIndex == 0 && !string.IsNullOrEmpty(m_name.stringValue))
				{
					height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				}
			}

            return height;
		}

		#endregion
	}
}