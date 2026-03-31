using System;
public enum ConditionType{Quest,Item,Dialogue};
// Created by Jantina
[Serializable]
public class DialogueCondition
{
    public ConditionType conditionType;
    public string questID;
    public string itemID;
    public Dialogue dialogue;

    public bool Evaluate(PlayerState player)
    {
        switch (conditionType)
        {
            case ConditionType.Quest:
                return player.HasQuest(questID);
            case ConditionType.Item:
                return player.HasItem(itemID);
            case ConditionType.Dialogue:
                return player.HasSeenDialogue(dialogue);
            default:
                return false;
        }
    }
}