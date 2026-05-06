using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas

/// <summary>
/// Triggers dialogue when the player has been idle for a set duration.
/// </summary>
public class IdleDialogueTrigger : MonoBehaviour
{
    public List<ConditionalDialogue> dialogues;

    [Header("How many seconds before it triggers dialogue")]
    [SerializeField] private float idleTime = 5f;

    [Header("Can it trigger only once?")]
    [SerializeField] private bool triggerOnce = true;

    private float lastInputTime;
    private bool hasTriggered = false;

    private void Start()
    {
        lastInputTime = Time.time;
    }

    private void Update()
    {
        if (IsPlayerActing())
        {
            lastInputTime = Time.time;
            return;
        }

        if (triggerOnce && hasTriggered) return;

        if (DialogueSystem.Instance.isDialogueActive) return;

        if (Time.time - lastInputTime >= idleTime)
        {
            StartDialogue();
            hasTriggered = true;
        }
    }

    private bool IsPlayerActing()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        return Input.anyKey ||
              Mathf.Abs(mouseX) > 0.01f ||
              Mathf.Abs(mouseY) > 0.01f;
    }

    private void StartDialogue()
    {
        for (int i = dialogues.Count - 1; i >= 0; i--)
        {
            var entry = dialogues[i];

            bool valid = true;

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

        Debug.LogWarning("No valid idle dialogue found.");
    }
}