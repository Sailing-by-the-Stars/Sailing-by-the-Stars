using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private List<QuestProgress> activeQuests = new();
    public IReadOnlyList<QuestProgress> ActiveQuests => activeQuests;
    private QuestUI questUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (questUI == null)
        {
            questUI = FindFirstObjectByType<QuestUI>();
        }
    }

    /// <summary>
    /// Adds a quest to the active quest list.
    /// </summary>
    public void StartQuest(Quest quest)
    {
        activeQuests.Add(new QuestProgress(quest));

        questUI.UpdateQuestUI();
    }

    /// <summary>
    /// Registers when quest objective item has been collected and calls quest UI update. Also removes quests when completed
    /// </summary>
    public void RegisterItemCollected(int itemID)
    {
        bool updated = false;

        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.Objectives)
            {
                if (obj.ObjectiveID == itemID)
                {
                    obj.AddProgress();
                    updated = true;
                }
            }
        }

        if (updated)
        {
            activeQuests.RemoveAll(q => q.IsCompleted);

            questUI.UpdateQuestUI();
        }
    }
}