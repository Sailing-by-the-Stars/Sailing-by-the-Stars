using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "NPCScriptables/Dialogue")]
public class Dialogue : ScriptableObject
{
    public bool hasName;
    public string npcName;
    public string[] dialogueLines;
    [Tooltip("Affects how fast the NPC speaks")]
    [Range(0f, 1f)]
    public float talkingSpeed = 0.1f;
    public bool givesQuest;
    public Quest quest;
}
