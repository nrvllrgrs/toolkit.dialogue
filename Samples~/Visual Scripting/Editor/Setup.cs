using System;
using System.Collections.Generic;
using Yarn.Unity;
using Yarn.Unity.Legacy;
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
				#pragma warning disable CS0618 // Type or member is obsolete
				typeof(DialogueViewBase),
				#pragma warning restore CS0618 // Type or member is obsolete
				#pragma warning disable CS0612 // Type or member is obsolete
				typeof(OptionsListView),
				typeof(OptionView),
				#pragma warning restore CS0612 // Type or member is obsolete
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