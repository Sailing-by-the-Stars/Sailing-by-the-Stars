using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest ID")]
    [SerializeField] private string questID;
    [Header("Quest name")]
    [SerializeField] private string questName;
    [Header("Quest description")]
    [SerializeField] private string description;
    [Header("Quest objectives")]
    [SerializeField] private List<QuestObjective> objectives = new();

    public string QuestID => questID;
    public string QuestName => questName;
    public string Description => description;
    public IReadOnlyList<QuestObjective> Objectives => objectives;

    private void OnValidate() 
    {
        if (string.IsNullOrEmpty(questID)) 
        {
            questID = System.Guid.NewGuid().ToString();
        }
    }   
}