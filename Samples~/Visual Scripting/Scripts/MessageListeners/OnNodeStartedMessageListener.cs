using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[AddComponentMenu("")]
    public class OnNodeStartedMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunnerControl>()?.onNodeStarted.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnNodeStarted, gameObject, value);
        });
    }
}