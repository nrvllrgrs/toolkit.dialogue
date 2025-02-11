using UnityEngine.Timeline;

namespace ToolkitEngine.Dialogue
{
	[TrackClipType(typeof(DialogueTrack))]
    [TrackBindingType(typeof(DialogueRunnerControl))]
    public class DialogueTrack : TrackAsset
    { }
}