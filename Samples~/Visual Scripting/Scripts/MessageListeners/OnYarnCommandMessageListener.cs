using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[AddComponentMenu("")]
    public class OnYarnCommandMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunnerControl>()?.onCommand.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnYarnCommand, gameObject, value);
        });
    }
}