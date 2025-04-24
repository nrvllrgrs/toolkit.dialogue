using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue
{
    public abstract class TTSGenerator : ScriptableObject
    {
		[SerializeField]
		protected bool m_importAssets;

		[SerializeField]
		protected DefaultAsset m_directory;

		public abstract void Generate(YarnProject project, IEnumerable<StringTableEntry> entries);
    }
}