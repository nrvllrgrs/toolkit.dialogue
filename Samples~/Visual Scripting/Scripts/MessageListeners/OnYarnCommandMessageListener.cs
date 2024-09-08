using UnityEngine;
using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
    [AddComponentMenu("")]
    public class OnYarnCommandMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunner>()?.onCommand.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnYarnCommand, gameObject, value);
        });
    }
}