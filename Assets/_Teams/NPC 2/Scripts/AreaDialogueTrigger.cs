using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas

/// <summary>
/// Handles dialogue interaction when entering certain areas.
/// </summary>
public class AreaDialogueTrigger : MonoBehaviour
{
    public List<ConditionalDialogue> dialogues;

    [SerializeField] private bool triggerOnce = true;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggerOnce && hasTriggered) return;

        StartDialogue();

        hasTriggered = true;
    }

    private void StartDialogue()
    {
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
                DialogueSystem.Instance.StartDialogue(entry.dialogue, gameObject);
                return;
            }
        }

        Debug.LogWarning("No valid dialogue found for trigger.");
    }
}