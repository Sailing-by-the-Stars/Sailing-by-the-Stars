using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas

[System.Serializable]
public class QuestObjective
{
    [Header("Quest objective ID")]
    [SerializeField] private int objectiveID;

    [Header("Quest objective description")]
    [SerializeField] private string description;

    [Header("Collect item settings")]
    [SerializeField] private int requiredAmount;
    [SerializeField] private int currentAmount;

    public int ObjectiveID => objectiveID;
    public string Description => description;
    public int RequiredAmount => requiredAmount;
    public int CurrentAmount => currentAmount;

    public bool IsCompleted => currentAmount >= requiredAmount;

    public QuestObjective(QuestObjective template)
    {
        objectiveID = template.objectiveID;
        description = template.description;
        requiredAmount = template.requiredAmount;
        currentAmount = 0;
    }

    /// <summary>
    /// Adds progress to the quest objective counter in the UI.
    /// </summary>
    public void AddProgress(int amount = 1)
    {
        currentAmount = Mathf.Min(currentAmount + amount, requiredAmount);
    }
}