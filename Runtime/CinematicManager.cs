using System.Collections;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class CinematicManager : ConfigurableSubsystem<CinematicManager, CinematicManagerConfig>
    {
		#region Fields

		private string m_skipDestination = null;
		private bool m_waiting = false;

		private float m_remainingTime = 0f;
		private float m_timeout = 0f;

		private DialogueRunnerControl m_cinematicControl;

		#endregion

		#region Properties

		public string skipDestination
		{
			get => m_skipDestination;
			private set => m_skipDestination = value;
		}

		public bool canSkip => !string.IsNullOrWhiteSpace(m_skipDestination);

		public float remainingTime => m_remainingTime;
		public float normalizedRemainingTime => m_remainingTime / m_timeout;

		#endregion

		#region Methods

		protected override void Initialize()
		{
			DialogueManager.CastInstance.DialogueStarted += DialogueManager_DialogueStarted;
		}

		protected override void Terminate()
		{
			DialogueManager.CastInstance.DialogueStarted -= DialogueManager_DialogueStarted;
		}

		public void Skip()
		{
			if (!canSkip || m_cinematicControl == null)
				return;

			m_cinematicControl.Stop(true);
			m_cinematicControl.Play(skipDestination);
			skipDestination = null;
		}

		public void Continue()
		{
			m_waiting = true;
		}


		#endregion

		#region Callbacks

		private void DialogueManager_DialogueStarted(object sender, DialogueEventArgs e)
		{
			if (e.control.dialogueType != Config.dialogueType)
				return;

			// Store DialogueRunnerControl associated with "cinematic" DialogueType
			m_cinematicControl = e.control;
		}

		#endregion

		#region Skip

		[YarnCommand("skip")]
		public static void SetupSkip(string destinationNode)
		{
			CleanupSkip();

			// Store destination of skipping
			CastInstance.skipDestination = destinationNode;

			DialogueManager.CastInstance.NodeStarted += Skip_NodeStarted;
			DialogueManager.CastInstance.DialogueCompleted += Skip_DialogueCompleted;
		}

		private static void Skip_NodeStarted(object sender, DialogueEventArgs e)
		{
			if (!Equals(e.control, CastInstance.m_cinematicControl)
				|| !Equals(e.nodeName, CastInstance.skipDestination))
				return;

			CleanupSkip();
		}

		private static void Skip_DialogueCompleted(object sender, DialogueEventArgs e)
		{
			// If Cinematic Dialogue ends before reaching skip node, cleanup
			if (!Equals(e.control, CastInstance.m_cinematicControl))
				return;

			CleanupSkip();
		}

		private static void CleanupSkip()
		{
			// Clear destination...cannot skip anymore
			CastInstance.skipDestination = null;

			// Stop watching events
			DialogueManager.CastInstance.NodeStarted -= Skip_NodeStarted;
			DialogueManager.CastInstance.DialogueCompleted -= Skip_DialogueCompleted;
		}

		#endregion

		#region Wait For Continue

		[YarnCommand("waitForContinue")]
		public static IEnumerator WaitForContinue()
		{
			CastInstance.m_waiting = true;
			yield return new WaitUntil(() => CastInstance.m_waiting);
		}

		[YarnCommand("waitForContinueWithTimeout")]
		public static IEnumerator WaitForContinueWithTimeout(float timeout, string variableName)
		{
			CastInstance.m_waiting = true;
			CastInstance.m_remainingTime = CastInstance.m_timeout = timeout;

			while (CastInstance.m_remainingTime > 0f)
			{
				yield return null;

				if (!CastInstance.m_waiting)
				{
					SetVariable(variableName, true);
				}
				CastInstance.m_remainingTime -= Time.deltaTime;
			}

			CastInstance.m_waiting = false;
			SetVariable(variableName, false);
		}

		private static void SetVariable(string variableName, bool value)
		{
			if (CastInstance.m_cinematicControl == null)
				return;

			var variableStorage = CastInstance.m_cinematicControl.dialogueRunner.VariableStorage;
			variableStorage.SetValue(variableName, value);
		}

		#endregion
	}
}