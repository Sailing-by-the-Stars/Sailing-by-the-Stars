using UnityEngine;

// Created by Jantina
public class ItemDialogueTrigger : MonoBehaviour
{

    [SerializeField] private Dialogue dialogue;
    [SerializeField] private bool onlyTriggerOnce = true;

    private bool hasTriggered = false;

    public void TriggerDialogue()
    {
        if (dialogue == null) return;
                if (onlyTriggerOnce && hasTriggered) return;

        hasTriggered = true;
        DialogueSystem.Instance.QueueDialogue(dialogue, gameObject);
    }
}