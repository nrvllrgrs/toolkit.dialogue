using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[AddComponentMenu("")]
    public class OnDialogueStartedMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunnerControl>()?.onDialogueStarted.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnDialogueStarted, gameObject, value);
        });
    }
}