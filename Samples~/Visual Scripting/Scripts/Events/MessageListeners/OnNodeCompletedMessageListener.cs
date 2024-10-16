using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[AddComponentMenu("")]
    public class OnNodeCompletedMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunnerControl>()?.onNodeCompleted.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnNodeCompleted, gameObject, value);
        });
    }
}