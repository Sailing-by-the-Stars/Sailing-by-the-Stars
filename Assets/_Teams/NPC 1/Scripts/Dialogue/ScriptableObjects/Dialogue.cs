using System.Collections.Generic;
using UnityEngine;
// Created by Jantina
[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    public bool hasName;
    public string npcName;
    [Tooltip("Affects how fast the NPC speaks. Default: 0.95")]
    [Range(0.8f, 1f)]
    public float talkingSpeed = 0.95f;
    [SerializeReference]
    public List<DialogueNode> nodes = new();
}
