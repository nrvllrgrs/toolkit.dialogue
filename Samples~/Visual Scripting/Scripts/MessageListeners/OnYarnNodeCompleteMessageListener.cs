using UnityEngine;
using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
    [AddComponentMenu("")]
    public class OnYarnNodeCompleteMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunner>()?.onNodeComplete.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnYarnNodeComplete, gameObject, value);
        });
    }
}