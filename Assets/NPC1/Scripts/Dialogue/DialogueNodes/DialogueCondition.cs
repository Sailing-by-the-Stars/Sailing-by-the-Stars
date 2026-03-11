using System;
public enum ConditionType{Quest,Item};
// Created by Jantina
[Serializable]
public class DialogueCondition
{
    public ConditionType conditionType;
    public string questID;
    public string itemID;

    public bool Evaluate(PlayerState player)
    {
        switch (conditionType)
        {
            case ConditionType.Quest:
                return player.HasQuest(questID);
            case ConditionType.Item:
                return player.HasItem(itemID);
            default:
                return false;
        }
    }
}