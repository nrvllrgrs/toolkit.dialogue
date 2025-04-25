using UnityEngine;
using UnityEngine.Playables;

namespace ToolkitEngine.Dialogue
{
    public class DialogueClip : PlayableAsset
    {
		#region Properties
#if UNITY_EDITOR

		public string id { get; set; }
		public string text { get; set; }
		public double length { get; set; }
		public DialogueSpeakerType speakerType { get; set; }

#endif
		#endregion

		#region Methods

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			return ScriptPlayable<DialogueBehaviour>.Create(graph);
		}

		#endregion
	}
}