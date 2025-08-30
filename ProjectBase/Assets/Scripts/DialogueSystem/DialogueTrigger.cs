using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueToTrigger;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogueToTrigger);
    }
}
