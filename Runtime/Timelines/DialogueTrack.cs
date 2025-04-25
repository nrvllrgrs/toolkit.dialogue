using UnityEngine.Timeline;

namespace ToolkitEngine.Dialogue
{
    [TrackClipType(typeof(DialogueClip))]
    [TrackBindingType(typeof(TimelineRunnerControl))]
    public class DialogueTrack : TrackAsset
    { }
}