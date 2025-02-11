using System;
using System.Collections;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class CinematicManager : ConfigurableSubsystem<CinematicManager, CinematicManagerConfig>
    {
		#region Fields

		private bool m_skippable = false;
		private string m_skipDestination = null;
		private bool m_waiting = false;

		private float m_remainingTime = 0f;
		private float m_timeout = 0f;

		private DialogueRunnerControl m_cinematicControl;

		#endregion

		#region Events

		public event EventHandler<bool> SkippableChanged;

		#endregion

		#region Properties

		public string skipDestination
		{
			get => m_skipDestination;
			private set
			{
				// No change, skip
				if (m_skipDestination == value)
					return;

				m_skipDestination = value;
				skippable = !string.IsNullOrWhiteSpace(m_skipDestination);
			}
		}

		public bool skippable
		{
			get => m_skippable;
			private set
			{
				// No change, skip
				if (m_skippable == value)
					return;

				bool wasSkippable = skippable;
				m_skippable = value;

				if (wasSkippable != skippable)
				{
					SkippableChanged?.Invoke(this, !wasSkippable);
				}
			}
		}

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
			if (!skippable || m_cinematicControl == null)
				return;

			m_cinematicControl.Stop(true);
			if (!string.IsNullOrWhiteSpace(m_skipDestination))
			{
				m_cinematicControl.Play(skipDestination);
				skipDestination = null;
			}
		}

		public void Continue()
		{
			m_waiting = true;
		}


		#endregion

		#region Callbacks

		private void DialogueManager_DialogueStarted(object sender, DialogueEventArgs e)
		{
			if (Config == null)
				return;

			if (e.control.dialogueType != Config.dialogueType)
				return;

			// Store DialogueRunnerControl associated with "cinematic" DialogueType
			m_cinematicControl = e.control;
		}

		#endregion

		#region Skip

		[YarnCommand("skipAndEnd")]
		public static void SetupSkipAndEnd()
		{
			CleanupSkip();
			CastInstance.skippable = true;

			DialogueManager.CastInstance.DialogueCompleted += Skip_DialogueCompleted;
		}

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