using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Programmer: Boas

[System.Serializable]
public class QuestProgress
{
    private Quest quest;
    private List<QuestObjective> objectives;

    public Quest Quest => quest;
    public IReadOnlyList<QuestObjective> Objectives => objectives;

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);

    public string QuestID => quest.QuestID;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();

        foreach (var obj in quest.Objectives)
        {
            objectives.Add(new QuestObjective(obj));
        }
    }
}