using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[AddComponentMenu("")]
    public class OnDialogueCompletedMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunnerControl>()?.onDialogueCompleted.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnDialogueCompleted, gameObject, value);
        });
    }
}