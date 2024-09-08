using UnityEngine;
using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
    [AddComponentMenu("")]
    public class OnYarnDialogueStartMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunner>()?.onDialogueStart.AddListener(() =>
        {
            EventBus.Trigger(EventHooks.OnYarnDialogueStart, gameObject);
        });
    }
}