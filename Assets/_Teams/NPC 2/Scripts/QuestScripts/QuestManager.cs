using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas
// Edited by: Arch

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private PlayerState playerState;

    private List<QuestProgress> activeQuests = new();
    public IReadOnlyList<QuestProgress> ActiveQuests => activeQuests;

    // QuestUI kept as a field so existing scene references do not break,
    // but it is now a no-op (see QuestUI.cs)
    private QuestUI questUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerState = FindFirstObjectByType<PlayerState>();

    }

    /// <summary>
    /// Adds a quest to the active quest list and updates the journal page.
    /// </summary>
    public void StartQuest(Quest quest)
    {
        activeQuests.Add(new QuestProgress(quest));

        PushToJournal();
    }

    /// <summary>
    /// Registers when a quest objective item has been collected.
    /// Removes completed quests and updates the journal page.
    /// </summary>
    public void RegisterItemCollected(int itemID)
    {
        bool updated = false;
        List<QuestProgress> completedQuests = new();

        foreach (QuestProgress quest in activeQuests)
        {
            foreach (QuestObjective obj in quest.Objectives)
            {
                if (obj.ObjectiveID == itemID)
                {
                    obj.AddProgress();
                    updated = true;
                }
            }

            if (quest.IsCompleted)
            {
                playerState.completedQuests.Add(quest.QuestID);
                completedQuests.Add(quest);
            }
        }

        if (updated)
        {
            foreach (QuestProgress quest in completedQuests)
            {
                // Notify journal page: pass the GUID string and display name
                // so the completed page can show the quest name
                if (QuestJournalPage.Instance != null)
                {
                    QuestJournalPage.Instance.UpdateDisplay(
                        activeQuests,
                        quest.QuestID,
                        quest.Quest.QuestName
                    );
                }

                quest.Quest.InvokeCompleted();
            }

            activeQuests.RemoveAll(q => q.IsCompleted);

            // Final refresh after completed quests are removed from the active list
            PushToJournal();
        }
    }

    private void PushToJournal()
    {
        if (QuestJournalPage.Instance != null)
            QuestJournalPage.Instance.UpdateDisplay(activeQuests);
    }
    
}
 