using UnityEditor;
using UnityEngine;
using ToolkitEngine.Dialogue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
    [CustomPropertyDrawer(typeof(YarnNode))]
    public class YarnNodeDrawer : PropertyDrawer
    {
		#region Fields

		protected SerializedProperty m_project;
		protected SerializedProperty m_name;

		private List<string> m_paths = null;
		private Dictionary<Tuple<YarnProject, string>, string> m_tupleToPath = null;
		private Dictionary<string, Tuple<YarnProject, string>> m_pathToTuple = null;

		#endregion

		#region Methods

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (m_paths == null)
			{
				m_project = property.FindPropertyRelative(nameof(m_project));
				m_name = property.FindPropertyRelative(nameof(m_name));

				m_paths = new();
				m_tupleToPath = new();
				m_pathToTuple = new();

				foreach (var project in YarnEditorUtil.GetYarnProjects())
				{
					var importer = AssetUtil.LoadImporter<YarnProjectImporter>(project);
					if (importer == null)
						continue;

					foreach (var t in importer.GenerateStringsTable()
						.Select(x => new
						{
							script = Path.GetFileNameWithoutExtension(x.File),
							node = x.Node
						}).Distinct())
					{
						string path = $"{project.name}/{t.script}/{t.node}";
						var tuple = new Tuple<YarnProject, string>(project, t.node);

						m_paths.Add(path);
						m_tupleToPath.Add(tuple, path);
						m_pathToTuple.Add(path, tuple);
					}
				}

				m_paths.Sort();
				m_paths.Insert(0, "[Empty]");
			}

			int selectedIndex = 0;
			EditorGUI.BeginChangeCheck();
			{
				if (!string.IsNullOrWhiteSpace(m_name.stringValue)
					&& m_project.objectReferenceValue != null)
				{
					var tuple = new Tuple<YarnProject, string>(m_project.objectReferenceValue as YarnProject, m_name.stringValue);
					if (m_tupleToPath.TryGetValue(tuple, out var selectedPath))
					{
						selectedIndex = m_paths.IndexOf(selectedPath);
					}
				}

				var tooltipRect = position;
				tooltipRect.x += EditorGUIUtility.labelWidth;
				tooltipRect.width -= EditorGUIUtility.labelWidth;

				selectedIndex = EditorGUIRectLayout.Popup(ref position, label.text, selectedIndex, m_paths.ToArray());

				if (selectedIndex > 0 && m_pathToTuple.TryGetValue(m_paths[selectedIndex], out var selectedTuple))
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
				else if (m_pathToTuple.TryGetValue(m_paths[selectedIndex], out var tuple))
				{
					m_project.objectReferenceValue = tuple.Item1;
					m_name.stringValue = tuple.Item2;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}

		#endregion
	}
}