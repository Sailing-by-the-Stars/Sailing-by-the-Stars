using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventNode : DialogueNode
{
    public UnityEvent onEvent = new UnityEvent();
    public string nextNodeID;
}