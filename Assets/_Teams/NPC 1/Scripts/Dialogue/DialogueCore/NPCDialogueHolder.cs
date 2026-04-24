using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConditionalDialogue
{
    public Dialogue dialogue;
    public List<DialogueCondition> conditions;
}
/// <summary>
/// Handles dialogue interaction with this NPC.
/// Ensures only the closest NPC can be interacted with.
/// </summary>
public class NPCDialogueHolder : MonoBehaviour, IInteractable
{
    public List<ConditionalDialogue> dialogues;

    [HideInInspector]
    public bool hasInteractedBefore = false;
    [SerializeField] private string interactMessage = "Press E to Talk";
    public string InteractMessage => interactMessage;
    public void Interact(InteractionController interactionController)
    {
        StartConversation();
    }

    void StartConversation()
    {
        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().SetBool("IsTalking", true);

        // 👇 LOOP BACKWARDS (highest priority = last added)
        for (int i = dialogues.Count - 1; i >= 0; i--)
        {
            var entry = dialogues[i];

            bool valid = true;

            // If no conditions → treat as fallback
            if (entry.conditions != null && entry.conditions.Count > 0)
            {
                foreach (var cond in entry.conditions)
                {
                    if (!cond.Evaluate(PlayerState.Instance))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (valid)
            {
                Debug.Log(DialogueSystem.Instance.name);
                Debug.Log(entry.dialogue.npcName);
                Debug.Log(gameObject.name);
                DialogueSystem.Instance.StartDialogue(entry.dialogue, gameObject);
                return;
            }
        }

        Debug.LogWarning("No valid dialogue found for NPC.");
    }

    public void EndConversation()
    {
        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetBool("IsTalking", false);
        }
    }
}