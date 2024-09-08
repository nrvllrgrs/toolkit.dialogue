using UnityEngine;
using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
    [AddComponentMenu("")]
    public class OnYarnNodeStartMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunner>()?.onNodeStart.AddListener((value) =>
        {
            EventBus.Trigger(EventHooks.OnYarnNodeStart, gameObject, value);
        });
    }
}