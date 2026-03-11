using System;
using System.Collections.Generic;
// Created by Jantina
[Serializable]
public class ChoiceNode : DialogueNode
{
    public List<DialogueChoice> choices = new();
}