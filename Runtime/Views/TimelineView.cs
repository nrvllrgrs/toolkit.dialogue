using System;
using System.Collections;
using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	/// <summary>
	/// A Dialogue View that presents lines of dialogue, using Unity UI
	/// elements.
	/// </summary>
	public class TimelineView : DialogueViewBase
	{
		#region Fields

		[SerializeField]
		private DialogueViewBase[] m_dialogueViews;

		/// <summary>
		/// The current <see cref="LocalizedLine"/> that this line view is processing.
		/// </summary>
		private LocalizedLine m_currentLine;

		/// <summary>
		/// Indicates whether dialogue view is waiting for timeline signal
		/// </summary>
		private bool m_waitingForSignal = false;

		/// <summary>
		/// Indicates whether waiting for signal is skipped due to interruption
		/// </summary>
		private bool m_skipWaiting = false;

		private ActionInfo m_dismissal = new();
		private ActionInfo m_runLine = new();

		private TimelineRunnerControl m_timelineRunnerControl = null;

		#endregion

		#region Methods

		public void StartDialogue(TimelineRunnerControl dialogueRunnerControl)
		{
			m_timelineRunnerControl = dialogueRunnerControl; 
		}

		private IEnumerator ProcessInternal(ActionInfo info, Action<DialogueViewBase> viewAction, Action onCompleted)
		{
			info.count = 0;
			foreach (var view in m_dialogueViews)
			{
				if (view == null)
					continue;

				++info.count;
				viewAction.Invoke(view);
			}

			yield return new WaitUntil(() => info.count == 0);
			onCompleted?.Invoke();
		}

		/// <inheritdoc/>
		public override void DismissLine(Action onDismissalComplete)
		{
			m_currentLine = null;
			StartCoroutine(
				ProcessInternal(
					m_dismissal,
					(view) => view.DismissLine(ViewDismissalComplete),
					onDismissalComplete));
		}

		private void ViewDismissalComplete()
		{
			--m_dismissal.count;
		}

		/// <inheritdoc/>
		public override void InterruptLine(LocalizedLine dialogueLine, Action onInterruptLineFinished)
		{
			m_currentLine = dialogueLine;

			// Cancel all coroutines that we're currently running. This will
			// stop the RunLineInternal coroutine, if it's running.
			StopAllCoroutines();

			foreach (var view in m_dialogueViews)
			{
				if (view == null)
					continue;

				view.InterruptLine(dialogueLine, () => { });
			}

			onInterruptLineFinished();
		}

		/// <inheritdoc/>
		public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
		{
			// Stop any coroutines currently running on this line view (for
			// example, any other RunLine that might be running)
			StopAllCoroutines();

			// Begin running the line as a coroutine.
			StartCoroutine(RunLineInternal(dialogueLine, onDialogueLineFinished));
		}

		private IEnumerator RunLineInternal(LocalizedLine dialogueLine, Action onDialogueLineFinished)
		{
			m_currentLine = dialogueLine;

			// Need to wait for signal from Timeline before displaying line
			m_waitingForSignal = !m_skipWaiting;
			m_skipWaiting = false;

			yield return new WaitWhile(() => m_waitingForSignal);

			StartCoroutine(
				ProcessInternal(
					m_runLine,
					(view) => view.RunLine(dialogueLine, ViewDialogueLineFinished),
					onDialogueLineFinished));
		}

		private void ViewDialogueLineFinished()
		{
			--m_runLine.count;
		}

		public void Resume()
		{
			if (!m_waitingForSignal)
			{
				m_skipWaiting = true;
				UserRequestedViewAdvancement();
			}

			m_waitingForSignal = false;
		}

		/// <inheritdoc/>
		public override void UserRequestedViewAdvancement()
		{
			if (m_currentLine == null)
				return;

			foreach (var view in m_dialogueViews)
			{
				if (view == null)
					continue;

				view.UserRequestedViewAdvancement();
			}

			requestInterrupt?.Invoke();
		}

		/// <inheritdoc />
		/// <remarks>
		/// If a line is still being shown dismisses it.
		/// </remarks>
		public override void DialogueComplete()
		{
			foreach (var view in m_dialogueViews)
			{
				if (view == null)
					continue;

				view.DialogueComplete();
			}
		}

		#endregion

		#region Structures

		private class ActionInfo
		{
			public int count;
		}

		#endregion
	}
}
