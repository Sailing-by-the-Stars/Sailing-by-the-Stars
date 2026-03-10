using UnityEngine;

/// <summary>
/// Handles dialogue interaction with this NPC.
/// Ensures only the closest NPC can be interacted with.
/// </summary>
public class NPCDialogueHolder : MonoBehaviour
{
    [Tooltip("Dialogue shown the first time the player talks to this NPC.")]
    public Dialogue firstDialogue;

    [Tooltip("Enable if the NPC should say something different after the first interaction.")]
    public bool hasRepeatDialogue;

    [Tooltip("Optional dialogue shown after the first interaction.")]
    public Dialogue repeatDialogue;

    [HideInInspector]
    public bool hasInteractedBefore = false;

    [Tooltip("Distance the player must be within to talk to this NPC.")]
    public float interactionDistance = 2f;

    private Transform player;
    private bool isInteracting;
    private static NPCDialogueHolder currentClosestNPC;
    private static float closestDistance = Mathf.Infinity;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Player left range
        if (distance > interactionDistance)
        {
            if (currentClosestNPC == this)
            {
                currentClosestNPC = null;
                InteractionPromptUI.Instance.HidePrompt();
            }

            return;
        }

        // Determine closest NPC
        if (currentClosestNPC == null || distance < closestDistance)
        {
            currentClosestNPC = this;
            closestDistance = distance;
        }

        // Only closest NPC can interact
        if (currentClosestNPC != this) return;

        // Hide prompt if dialogue active
        if (DialogueSystem.Instance.isDialogueActive)
        {
            InteractionPromptUI.Instance.HidePrompt();
            return;
        }

        // Show prompt when in range
        InteractionPromptUI.Instance.ShowPrompt("Press Enter to Talk");

        if (Input.GetKeyDown(KeyCode.Return) && !DialogueSystem.Instance.isDialogueActive)
        {
            StartConversation();
        }
    }

    void StartConversation()
    {
        InteractionPromptUI.Instance.HidePrompt();
        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetBool("IsTalking", true);
        }
        if (!hasInteractedBefore || !hasRepeatDialogue)
        {
            DialogueSystem.Instance.StartDialogue(firstDialogue, this.gameObject);
            hasInteractedBefore = true;
        }
        else
        {
            DialogueSystem.Instance.StartDialogue(repeatDialogue, this.gameObject);
        }
    }

    public void EndConversation()
    {
        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().SetBool("IsTalking", false);
        }
    }
}