using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEditor.Dialogue
{
    public abstract class TTSGenerator : ScriptableObject
    {
        public abstract void Generate(YarnProject project, IEnumerable<StringTableEntry> entries);
    }
}