using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class TimelineManager : Subsystem<TimelineManager>
    {
		#region Fields

		private Dictionary<string, Timeline> m_timelineMap = new();
		private HashSet<PlayableDirector> m_playingDirectors = new();

		#endregion

		#region Events

		public event EventHandler<PlayableDirector> PlayableDirectorTracked;
		public event EventHandler<PlayableDirector> PlayableDirectorUntracked;

#if UNITY_EDITOR
		public Action<FrameData> DialoguePreviewClipPlayed;
#endif
		#endregion

		#region Methods

		[YarnCommand("startTimeline")]
		public static void StartTimeline(string timelineKey)
		{
			if (CastInstance.m_timelineMap.TryGetValue(timelineKey, out var timeline))
			{
				CastInstance.m_playingDirectors.Add(timeline.playableDirector);
				timeline.playableDirector.Play();
				timeline.playableDirector.stopped += PlayableDirector_Stopped;

				CastInstance.PlayableDirectorTracked?.Invoke(CastInstance, timeline.playableDirector);
			}
		}

		private static void PlayableDirector_Stopped(PlayableDirector playableDirector)
		{
			playableDirector.stopped -= PlayableDirector_Stopped;
			CastInstance.m_playingDirectors.Remove(playableDirector);

			CastInstance.PlayableDirectorUntracked?.Invoke(CastInstance, playableDirector);
		}

		public void Register(Timeline timeline)
		{
			if (timeline == null || m_timelineMap.ContainsKey(timeline.key))
				return;

			m_timelineMap.Add(timeline.key, timeline);
		}

		public void Unregister(Timeline timeline)
		{
			if (timeline == null || !m_timelineMap.ContainsKey(timeline.key))
				return;

			m_timelineMap.Remove(timeline.key);
		}

		#endregion
	}
}