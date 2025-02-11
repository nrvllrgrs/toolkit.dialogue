using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ToolkitEngine.Dialogue
{
	public class CinematicRunnerControl : DialogueRunnerControl, INotificationReceiver
	{
		#region Fields

		[SerializeField]
		private SignalAsset m_signal;

		private HashSet<TimelineView> m_signalViews = new();

		#endregion

		#region Events

		[SerializeField]
		private UnityEvent<DialogueEventArgs> m_onCinematicSkipped;

		#endregion

		#region Properties

		public UnityEvent<DialogueEventArgs> onCinematicSkipped => m_onCinematicSkipped;

		#endregion

		#region Methods

		protected override void Awake()
		{
			base.Awake();
			foreach (var view in dialogueRunner.dialogueViews)
			{
				if (view is not TimelineView timelineView)
					continue;

				m_signalViews.Add(timelineView);
			}
		}

		public void OnNotify(Playable origin, INotification notification, object context)
		{
			if (notification is not SignalEmitter signal || signal?.asset == null)
				return;

			foreach (var view in m_signalViews)
			{
				view.Resume();
			}
		}

		public override void Stop(bool skipping)
		{
			if (!isDialogueRunning)
				return;

			m_isSkipping = skipping;
			dialogueRunner.Stop();

			if (skipping)
			{
				onCinematicSkipped?.Invoke(new DialogueEventArgs(this));
			}
		}

		#endregion
	}
}