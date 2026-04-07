using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Created by Jantina
// Placeholder script
public class PlayerState : MonoBehaviour
{

    public static PlayerState Instance;
    public List<string> completedQuests = new List<string>();
    public List<string> inventory = new List<string>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate PlayerState destroyed");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public bool HasQuest(string questID) => completedQuests.Contains(questID);
    public bool HasItem(string itemID) => inventory.Contains(itemID);
    public List<Dialogue> completedDialogues = new List<Dialogue>();

    public bool HasSeenDialogue(Dialogue dialogue) => completedDialogues.Contains(dialogue);

    public void MarkDialogueComplete(Dialogue dialogue)
    {
        if (!completedDialogues.Contains(dialogue))
            completedDialogues.Add(dialogue);
    }
    public void GetItem(string itemID)
    {
        inventory.Add(itemID);

    }
}