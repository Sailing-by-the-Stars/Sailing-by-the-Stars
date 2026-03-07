using UnityEngine;
using System;
using System.Collections.Generic;
// Created By Jantina
public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private Dialogue currentDialogue;
    [SerializeField] private DialogueUIManager uiManager;
    [SerializeField] private PlayerState player;

    private Dictionary<string, DialogueNode> nodeLookup;
    [SerializeField] Dialogue testingDialogue;
    [HideInInspector] public DialogueNode currentNode;
    private DialogueLineNode currentLineNode;
    private bool lineFullyRevealed = false;
    private bool waitingForPlayerInput = false;

    private void Awake()
    {
        if (player == null)
        {
            GameObject placeholder = new GameObject("PlayerState");
            player = placeholder.AddComponent<PlayerState>();
        }
    }

    private void Start()
    {
        if (currentDialogue == null || uiManager == null) return;
        BuildNodeLookup();
        StartDialogue(testingDialogue);
    }

    private void Update()
    {
        if (!waitingForPlayerInput) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (uiManager.TypewriterRunning)
            {
                uiManager.SkipTypewriter();
                lineFullyRevealed = true;
                return;
            }

            if (currentNode is ChoiceNode)
            {
                return;
            }

            if (currentNode is ConditionalNode condNodeClick)
            {
                bool allPass = true;
                foreach (var condition in condNodeClick.conditions)
                {
                    if (!condition.Evaluate(player))
                    {
                        allPass = false;
                        break;
                    }
                }

                string nextID = allPass ? condNodeClick.trueNodeID : condNodeClick.falseNodeID;

                if (string.IsNullOrEmpty(nextID))
                {
                    uiManager.EndDialogue();
                    currentNode = null;
                }
                else if (!nodeLookup.TryGetValue(nextID, out currentNode))
                {
                    Debug.LogWarning($"Node '{nextID}' not found. Ending dialogue.");
                    uiManager.EndDialogue();
                    currentNode = null;
                }
                else
                {
                    waitingForPlayerInput = false;
                    ProcessNode();
                }

                return; 
            }

            waitingForPlayerInput = false;
            string nextIDNormal = GetNextNodeIDForCurrentNode();
            AdvanceNode(nextIDNormal);
        }
    }
    private void BuildNodeLookup()
    {
        nodeLookup = new Dictionary<string, DialogueNode>();
        foreach (var node in currentDialogue.nodes)
        {
            if (node != null && !string.IsNullOrEmpty(node.nodeID))
                nodeLookup[node.nodeID] = node;
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        if (currentDialogue.nodes.Count == 0) return;
        currentNode = currentDialogue.nodes[0];
        ProcessNode();
    }

    private void ProcessNode()
    {
        if (currentNode == null)
        {
            uiManager.EndDialogue();
            return;
        }

        string npcName = currentDialogue.hasName ? currentDialogue.npcName : "";

        currentLineNode = null;
        lineFullyRevealed = false;
        waitingForPlayerInput = true;

        float speed = Mathf.Lerp(0.5f, 0.01f, currentDialogue.talkingSpeed);

        if (currentNode is DialogueLineNode lineNode)
        {
            currentLineNode = lineNode;
            uiManager.ShowDialogueNode(lineNode, npcName, speed, OnTypewriterComplete);
        }
        else if (currentNode is ChoiceNode choiceNode)
        {
            waitingForPlayerInput = true; //
            currentLineNode = new DialogueLineNode { text = choiceNode.text }; 
            uiManager.ShowChoiceNode(choiceNode, npcName, OnChoiceSelected, speed);
        }
        else if (currentNode is ConditionalNode condNode)
        {
            
            currentLineNode = new DialogueLineNode { text = condNode.text };
            lineFullyRevealed = false;
            waitingForPlayerInput = true;

            
            string trueID = condNode.trueNodeID;
            string falseID = condNode.falseNodeID;
            var conditions = condNode.conditions;

            uiManager.ShowDialogueNode(currentLineNode, currentDialogue.hasName ? currentDialogue.npcName : "", speed, OnTypewriterComplete);

        }
    }

    private void OnTypewriterComplete()
    {
        lineFullyRevealed = true;
        
    }

    private string GetNextNodeIDForCurrentNode()
    {
        if (currentNode is DialogueLineNode lineNode)
            return lineNode.nextNodeID;
        return null;
    }

    private void AdvanceNode(string nextNodeID)
    {
        if (string.IsNullOrEmpty(nextNodeID))
        {
            uiManager.EndDialogue();
            currentNode = null;
            return;
        }

        if (!nodeLookup.TryGetValue(nextNodeID, out currentNode))
        {
            Debug.LogWarning($"Node '{nextNodeID}' not found. Ending dialogue.");
            uiManager.EndDialogue();
            currentNode = null;
            return;
        }

        ProcessNode();
    }

    private void OnChoiceSelected(string nextNodeID)
    {
        if (uiManager.TypewriterRunning)
        {
            uiManager.SkipTypewriter();
            return;
        }

        if (string.IsNullOrEmpty(nextNodeID))
        {
            uiManager.EndDialogue();
            currentNode = null;
            return;
        }

        if (!nodeLookup.TryGetValue(nextNodeID, out currentNode))
        {
            Debug.LogWarning($"Node '{nextNodeID}' not found. Ending dialogue.");
            uiManager.EndDialogue();
            currentNode = null;
            return;
        }

        ProcessNode();
    }
}