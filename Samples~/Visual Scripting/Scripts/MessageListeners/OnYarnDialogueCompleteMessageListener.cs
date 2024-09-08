using UnityEngine;
using Unity.VisualScripting;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue.VisualScripting
{
    [AddComponentMenu("")]
    public class OnYarnDialogueCompleteMessageListener : MessageListener
    {
        private void Start() => GetComponent<DialogueRunner>()?.onDialogueComplete.AddListener(() =>
        {
            EventBus.Trigger(EventHooks.OnYarnDialogueComplete, gameObject);
        });
    }
}