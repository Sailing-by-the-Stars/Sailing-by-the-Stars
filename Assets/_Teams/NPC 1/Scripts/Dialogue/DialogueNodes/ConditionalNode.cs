using System;
using System.Collections.Generic;
using UnityEngine;
// Created by Jantina
[Serializable]
public class ConditionalNode : DialogueNode
{
    [SerializeField]
    public List<DialogueCondition> conditions = new List<DialogueCondition>();

    public string trueNodeID;
    public string falseNodeID;

}