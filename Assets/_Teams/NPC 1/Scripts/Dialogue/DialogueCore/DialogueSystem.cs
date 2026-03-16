using UnityEngine;
using System;
using System.Collections.Generic;
// Created by Jantina

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue References")]

    [Tooltip("The dialogue asset currently being processed.")]
    [SerializeField] private Dialogue currentDialogue;

    [Tooltip("Handles all dialogue UI such as text, choices and typewriter effects.")]
    [SerializeField] private DialogueUIManager uiManager;

    [Tooltip("Reference to the player state used for evaluating dialogue conditions.")]
    [SerializeField] private PlayerState player;

    public static DialogueSystem Instance;
    private Dictionary<string, DialogueNode> nodeLookup;

    [HideInInspector] public DialogueNode currentNode;
    private DialogueLineNode currentLineNode;
    private bool lineFullyRevealed = false;
    [HideInInspector] public bool isDialogueActive = false;
    private GameObject sendingObject;
    private bool waitingForPlayerInput = false;

    private void Awake()
    {
        Instance = this;

        if (player == null)
        {
            GameObject placeholder = new GameObject("PlayerState");
            player = placeholder.AddComponent<PlayerState>();
        }
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
                return;

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
                    if (sendingObject.GetComponent<NPCDialogueHolder>() != null)
                    {
                        sendingObject.GetComponent<NPCDialogueHolder>().EndConversation();
                    }
                    currentNode = null;
                }
                else if (!nodeLookup.TryGetValue(nextID, out currentNode))
                {
                    Debug.LogWarning($"Node '{nextID}' not found. Ending dialogue.");
                    uiManager.EndDialogue();
                    if (sendingObject.GetComponent<NPCDialogueHolder>() != null)
                    {
                        sendingObject.GetComponent<NPCDialogueHolder>().EndConversation();
                    }
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

    /// <summary>
    /// Builds a lookup dictionary for quickly retrieving dialogue nodes by ID.
    /// </summary>
    private void BuildNodeLookup()
    {
        nodeLookup = new Dictionary<string, DialogueNode>();

        foreach (var node in currentDialogue.nodes)
        {
            if (node != null && !string.IsNullOrEmpty(node.nodeID))
                nodeLookup[node.nodeID] = node;
        }
    }

    /// <summary>
    /// Starts a dialogue sequence using the provided dialogue asset.
    /// </summary>
    public void StartDialogue(Dialogue dialogue, GameObject sender)
    {
        if (currentDialogue == null || uiManager == null) return;

        sendingObject = sender;
        currentDialogue = dialogue;
        BuildNodeLookup();
        isDialogueActive = true;
        if (currentDialogue.hasItemID == true) QuestManager.Instance.RegisterItemCollected(currentDialogue.itemID);

        if (currentDialogue.nodes.Count == 0) return;

        currentNode = currentDialogue.nodes[0];
        ProcessNode();
    }

    /// <summary>
    /// Processes the current dialogue node and determines which UI or logic should be executed.
    /// </summary>
    private void ProcessNode()
    {
        if (currentNode == null)
        {
            uiManager.EndDialogue();
            if (sendingObject.GetComponent<NPCDialogueHolder>() != null)
            {
                sendingObject.GetComponent<NPCDialogueHolder>().EndConversation();
            }
            
            return;
        }

        string npcName = currentDialogue.hasName ? currentDialogue.npcName : "";

        currentLineNode = null;
        lineFullyRevealed = false;
        waitingForPlayerInput = true;

        float speed = 1 - currentDialogue.talkingSpeed;

        if (currentNode is DialogueLineNode lineNode)
        {
            currentLineNode = lineNode;
            uiManager.ShowDialogueNode(lineNode, npcName, speed, OnTypewriterComplete);
        }
        else if (currentNode is ChoiceNode choiceNode)
        {
            waitingForPlayerInput = true;
            currentLineNode = new DialogueLineNode { text = choiceNode.text };

            uiManager.ShowChoiceNode(choiceNode, npcName, OnChoiceSelected, speed);
        }
        else if (currentNode is ConditionalNode condNode)
        {
            currentLineNode = new DialogueLineNode { text = condNode.text };

            lineFullyRevealed = false;
            waitingForPlayerInput = true;

            uiManager.ShowDialogueNode(
                currentLineNode,
                currentDialogue.hasName ? currentDialogue.npcName : "",
                speed,
                OnTypewriterComplete
            );
        }
        else if (currentNode is StartQuestNode questNode)
        {
            if (questNode.questToStart != null)
            {
                QuestManager.Instance.StartQuest(questNode.questToStart);
            }
            currentLineNode = new DialogueLineNode { text = questNode.text };
            lineFullyRevealed = false;
            waitingForPlayerInput = true;

            uiManager.ShowDialogueNode(
                currentLineNode,
                currentDialogue.hasName ? currentDialogue.npcName : "",
                speed,
                OnTypewriterComplete
            );
        }
    }

    /// <summary>
    /// Called when the dialogue typewriter effect finishes revealing the current text.
    /// </summary>
    private void OnTypewriterComplete()
    {
        lineFullyRevealed = true;
    }

    /// <summary>
    /// Retrieves the next node ID from the current dialogue line node.
    /// </summary>
    private string GetNextNodeIDForCurrentNode()
    {
        if (currentNode is DialogueLineNode lineNode)
            return lineNode.nextNodeID;

        return null;
    }

    /// <summary>
    /// Advances the dialogue to the node with the provided ID.
    /// </summary>
    private void AdvanceNode(string nextNodeID)
    {
        if (string.IsNullOrEmpty(nextNodeID))
        {
            uiManager.EndDialogue();
            if (sendingObject.GetComponent<NPCDialogueHolder>() != null)
            {
                sendingObject.GetComponent<NPCDialogueHolder>().EndConversation();
            }
            currentNode = null;
            return;
        }

        if (!nodeLookup.TryGetValue(nextNodeID, out currentNode))
        {
            Debug.LogWarning($"Node '{nextNodeID}' not found. Ending dialogue.");
            uiManager.EndDialogue();
            if (sendingObject.GetComponent<NPCDialogueHolder>() != null)
            {
                sendingObject.GetComponent<NPCDialogueHolder>().EndConversation();
            }
            currentNode = null;
            return;
        }

        ProcessNode();
    }

    /// <summary>
    /// Handles the player selecting a dialogue choice and advances to the linked node.
    /// </summary>
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
            if (sendingObject.GetComponent<NPCDialogueHolder>() != null)
            {
                sendingObject.GetComponent<NPCDialogueHolder>().EndConversation();
            }
            currentNode = null;
            return;
        }

        if (!nodeLookup.TryGetValue(nextNodeID, out currentNode))
        {
            Debug.LogWarning($"Node '{nextNodeID}' not found. Ending dialogue.");
            uiManager.EndDialogue();
            if (sendingObject.GetComponent<NPCDialogueHolder>() != null)
            {
                sendingObject.GetComponent<NPCDialogueHolder>().EndConversation();
            }
            currentNode = null;
            return;
        }

        ProcessNode();
    }
}