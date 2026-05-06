using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas

/// <summary>
/// Triggers dialogue when the player is idle inside a trigger area.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AreaIdleDialogueTrigger : MonoBehaviour
{
    public List<ConditionalDialogue> dialogues;

    [SerializeField] private float idleTime = 5f;
    [SerializeField] private bool triggerOnce = true;

    private float lastInputTime;
    private bool hasTriggered = false;
    private bool playerInside = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (!playerInside) return;

        if (IsPlayerActing())
        {
            lastInputTime = Time.time;

            if (!triggerOnce)
                hasTriggered = false;

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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        lastInputTime = Time.time;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
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

        Debug.LogWarning("No valid idle dialogue found for this area.");
    }
}