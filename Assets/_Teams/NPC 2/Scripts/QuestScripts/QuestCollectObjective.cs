using UnityEngine;

// Programmer: Boas

public class CollectQuestItem : MonoBehaviour, IInteractable
{
    [Header("Quest Objective ID")]
    [SerializeField] private int itemID;

    [Header("Text field")]
    [SerializeField] private string interactPrompt = "Press E to Interact";

    public string InteractMessage => interactPrompt;

    [Header("Delete when picked up?")]
    [SerializeField] private bool destroyOnCollect = true;

    /// <summary>
    /// Handles collecting the quest item and registering progress with the QuestManager.
    /// </summary>
    private void Collect()
    {
        QuestManager.Instance.RegisterItemCollected(itemID);

        if (destroyOnCollect)
        {
            InteractionPromptUI.Instance.HidePrompt();
            Destroy(transform.root.gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    public void Interact(InteractionController interactionController)
    {
        Collect();
    }
}