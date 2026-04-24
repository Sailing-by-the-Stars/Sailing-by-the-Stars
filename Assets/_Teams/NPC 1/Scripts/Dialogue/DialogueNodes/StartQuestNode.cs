using UnityEngine;

[System.Serializable]
public class StartQuestNode : DialogueNode
{
    [Tooltip("Quest that will be started when this node is reached.")]
    public Quest questToStart;

    [Tooltip("Next node after starting the quest.")]
    public string nextNodeID;
}