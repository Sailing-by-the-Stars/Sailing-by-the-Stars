using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventNode : DialogueNode
{
    public string eventID;
    public string nextNodeID;
}