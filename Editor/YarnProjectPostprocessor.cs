using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Yarn.Unity;
using Yarn.Unity.Editor;

namespace ToolkitEditor.Dialogue
{
	public class YarnProjectPostprocessor : AssetPostprocessor
	{
		#region Fields

		private static string[] s_pathsArray = null;
		private static List<string> s_paths = null;
		private static Dictionary<Tuple<YarnProject, string>, string> s_tupleToPath = null;
		private static Dictionary<string, Tuple<YarnProject, string>> s_pathToTuple = null;

		#endregion

		#region Properties

		public static string[] Paths => s_pathsArray;

		#endregion

		#region Methods

		[InitializeOnLoadMethod]
		static void Initialize()
		{
			CacheNodeNames();
		}

		static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			foreach (var assetPath in importedAssets)
			{
				if (!assetPath.EndsWith(".yarnproject"))
					continue;

				var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(assetPath);
				if (yarnProject == null)
					continue;

				CacheNodeNames();
				break;
			}
		}

		private static void CacheNodeNames()
		{
			s_paths = new();
			s_tupleToPath = new();
			s_pathToTuple = new();

			foreach (var project in YarnEditorUtil.GetYarnProjects())
			{
				var importer = AssetUtil.LoadImporter<YarnProjectImporter>(project);
				if (importer == null)
					continue;

				//foreach (var t in project.NodeNames
				//	.Select(x => new
				//	{
				//		script = YarnEditorUtil.FindYarnScript(project, x)?.name,
				//		node = x,
				//	}).Distinct())
				foreach (var t in YarnEditorUtil.GenerateStringsTable(importer)
					.Select(x => new
					{
						script = Path.GetFileNameWithoutExtension(x.File),
						node = x.Node
					}).Distinct())
				{
					if (string.IsNullOrEmpty(t.script))
						continue;

					string path = $"{project.name}/{t.script}/{t.node}";
					var tuple = new Tuple<YarnProject, string>(project, t.node);

					// Possible when using WHEN conditions on node
					if (s_tupleToPath.ContainsKey(tuple))
						continue;

					s_paths.Add(path);
					s_tupleToPath.Add(tuple, path);
					s_pathToTuple.Add(path, tuple);
				}
			}

			s_paths.Sort();
			s_paths.Insert(0, "[Empty]");
			s_pathsArray = s_paths.ToArray();
		}

		public static int IndexOfPath(string path)
		{
			return s_paths.IndexOf(path);
		}

		public static bool TryGetYarnProjectTuple(string path, out Tuple<YarnProject, string> tuple)
		{
			return s_pathToTuple.TryGetValue(path, out tuple);
		}

		public static bool TryGetPath(Tuple<YarnProject, string> tuple, out string path)
		{
			return s_tupleToPath.TryGetValue(tuple, out path);
		}

		#endregion
	}
}