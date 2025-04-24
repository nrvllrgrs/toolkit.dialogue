using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue
{
    public abstract class TTSGenerator : ScriptableObject
    {
		#region Fields

		[SerializeField]
		protected bool m_importAssets;

		[SerializeField]
		protected DefaultAsset m_directory;

		#endregion

		#region Methods

		public abstract void Generate(YarnProject project, IEnumerable<StringTableEntry> entries);

		#endregion
	}
}