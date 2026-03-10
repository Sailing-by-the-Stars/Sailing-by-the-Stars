using System.Collections.Generic;
using UnityEngine;
// Created by Jantina
[CreateAssetMenu(fileName = "Dialogue", menuName = "NPCScriptables/Dialogue")]
public class Dialogue : ScriptableObject
{
    public bool hasName;
    public string npcName;
    [Tooltip("Affects how fast the NPC speaks")]
    [Range(0.1f, 1f)]
    public float talkingSpeed = 0.1f;
    [SerializeReference]
    public List<DialogueNode> nodes = new();
}
