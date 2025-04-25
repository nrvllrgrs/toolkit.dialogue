using UnityEngine;
using UnityEngine.Playables;

namespace ToolkitEngine.Dialogue
{
    public class DialogueBehaviour : PlayableBehaviour
    {
		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				TimelineManager.CastInstance.DialoguePreviewClipPlayed?.Invoke(info);
				return;
			}
#endif

			if (info.output.GetUserData() is TimelineRunnerControl control)
			{
				control?.Resume();
			}
		}
	}
}