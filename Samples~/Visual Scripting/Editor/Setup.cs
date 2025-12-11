using System;
using System.Collections.Generic;
using Yarn.Unity;
using UnityEditor;
using ToolkitEngine.Dialogue;

namespace ToolkitEditor.Dialogue.VisualScripting
{
	[InitializeOnLoad]
	public static class Setup
	{
		static Setup()
		{
			var types = new List<Type>()
			{
				// Yarn
				typeof(DialogueRunner),
				typeof(YarnProject),
				typeof(DialoguePresenterBase),
			};

			ToolkitEditor.VisualScripting.Setup.Initialize("Yarn.Unity", types);

			types = new List<Type>()
			{
				// Dialogue
				typeof(DialogueManager),
				typeof(DialogueManagerConfig),
				typeof(DialogueRunnerControl),
				typeof(DialogueCategory),
				typeof(DialogueType),
				typeof(DialogueEventArgs),

				// Nudges
				typeof(NudgeType),
			};

			ToolkitEditor.VisualScripting.Setup.Initialize("ToolkitEngine.Dialogue", types);
		}
	}
}