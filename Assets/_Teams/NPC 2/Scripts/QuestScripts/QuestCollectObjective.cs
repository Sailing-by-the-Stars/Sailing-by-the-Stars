using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

// Programmer: Boas

public class CollectQuestItem : MonoBehaviour
{
    [Header("Quest Objective ID")]
    [SerializeField] private int itemID;

    [Header("Text field")]
    [SerializeField] private TMP_Text interactPrompt;

    [Header("Delete when picked up?")]
    [SerializeField] private bool destroyOnCollect = true;

    private bool playerInRange = false;

    private void Start()
    {
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Collect();
        }
    }

    /// <summary>
    /// Handles collecting the quest item and registering progress with the QuestManager.
    /// </summary>
    private void Collect()
    {
        QuestManager.Instance.RegisterItemCollected(itemID);

        if (interactPrompt != null) 
        {
            interactPrompt.gameObject.SetActive(false);
        }

        if (destroyOnCollect)
        {
            Destroy(transform.root.gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactPrompt != null)
            {
                interactPrompt.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.gameObject.SetActive(false);
            }
        }
    }
}