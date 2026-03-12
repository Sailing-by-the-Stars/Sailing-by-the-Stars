using System;
using UnityEngine;
// Created by Jantina
[Serializable]
public abstract class DialogueNode
{
    public string nodeID;
    
    [TextArea(3,6)]
    public string text;

}
