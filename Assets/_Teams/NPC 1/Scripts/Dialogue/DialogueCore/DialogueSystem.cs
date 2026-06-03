using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
// Created by Jantina

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue References")]
    [SerializeField] private Dialogue currentDialogue;
    [SerializeField] private DialogueUIManager uiManager;

    public EventReference CurrentVoice { get; private set; }
    public static DialogueSystem Instance;

    private Dictionary<string, DialogueNode> nodeLookup;

    [HideInInspector] public DialogueNode currentNode;
    private DialogueLineNode currentLineNode;
    private bool lineFullyRevealed = false;
    [HideInInspector] public bool isDialogueActive = false;
    private GameObject sendingObject;
    private bool waitingForPlayerInput = false;
    private PlayerControls controls;
    private PlayerControls.DialogueActions dialogueControls;

    private Queue<(Dialogue dialogue, GameObject sender)> dialogueQueue = new();
    private Coroutine autoAdvanceCoroutine;

    private void Awake()
    {
        Instance = this;
        uiManager = FindFirstObjectByType<DialogueUIManager>();
        controls = TempStateMachine.Instance.PlayerControls;
        dialogueControls = controls.Dialogue;
    }

    private void Start()
    {
        dialogueControls.Advance.performed += OnAdvancePerformed;
        dialogueControls.Choice1.performed += OnChoice1Performed;
        dialogueControls.Choice2.performed += OnChoice2Performed;
        dialogueControls.FastForward.performed += OnFastForwardStarted;
        dialogueControls.FastForward.canceled += OnFastForwardCanceled;
    }

    private void OnDisable()
    {
        dialogueControls.Advance.performed -= OnAdvancePerformed;
        dialogueControls.Choice1.performed -= OnChoice1Performed;
        dialogueControls.Choice2.performed -= OnChoice2Performed;
        dialogueControls.FastForward.performed -= OnFastForwardStarted;
        dialogueControls.FastForward.canceled -= OnFastForwardCanceled;
    }

    private void OnChoice1Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!isDialogueActive) return;
        uiManager.TriggerChoice1();
    }

    private void OnChoice2Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!isDialogueActive) return;
        uiManager.TriggerChoice2();
    }

    private void OnFastForwardStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        uiManager.SetFastForward(true);
    }

    private void OnFastForwardCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        uiManager.SetFastForward(false);
    }

    private void OnAdvancePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (currentDialogue != null && currentDialogue.autoAdvance) return;
        if (!waitingForPlayerInput) return;

        if (uiManager.TypewriterRunning)
        {
            uiManager.SkipTypewriter();
            lineFullyRevealed = true;
            return;
        }

        if (!lineFullyRevealed) return;
        if (currentNode is ChoiceNode) return;

        if (currentNode is ConditionalNode condNodeClick)
        {
            bool allPass = true;
            foreach (var condition in condNodeClick.conditions)
            {
                if (!condition.Evaluate(PlayerState.Instance))
                {
                    allPass = false;
                    break;
                }
            }

            string nextID = allPass ? condNodeClick.trueNodeID : condNodeClick.falseNodeID;

            if (string.IsNullOrEmpty(nextID))
                EndDialogue();
            else if (!nodeLookup.TryGetValue(nextID, out currentNode))
            {
                Debug.LogWarning($"Node '{nextID}' not found. Ending dialogue.");
                EndDialogue();
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

    private void BuildNodeLookup()
    {
        nodeLookup = new Dictionary<string, DialogueNode>();
        foreach (var node in currentDialogue.nodes)
            if (node != null && !string.IsNullOrEmpty(node.nodeID))
                nodeLookup[node.nodeID] = node;
    }

    public void QueueDialogue(Dialogue dialogue, GameObject sender)
    {
        if (dialogue == null) return;

        if (isDialogueActive)
        {
            dialogueQueue.Enqueue((dialogue, sender));
        }
        else
        {
            StartDialogue(dialogue, sender);
        }
    }

    public void StartDialogue(Dialogue dialogue, GameObject sender)
    {
        if (dialogue == null || uiManager == null) return;

        if (!dialogue.autoAdvance)
            TempStateMachine.Instance.SetState(GameState.Dialogue);

        sendingObject = sender;
        currentDialogue = dialogue;

        var npc = sender.GetComponent<NPCDialogueHolder>();
        CurrentVoice = npc != null ? npc.dialogueVoice : default;

        BuildNodeLookup();
        isDialogueActive = true;

        if (currentDialogue.hasItemID)
            QuestManager.Instance.RegisterItemCollected(currentDialogue.itemID);

        if (currentDialogue.nodes.Count == 0) return;

        currentNode = currentDialogue.nodes[0];
        ProcessNode();
    }

    private void ProcessNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }

        string npcName = currentDialogue.hasName ? currentDialogue.npcName : "";
        currentLineNode = null;
        lineFullyRevealed = false;
        waitingForPlayerInput = true;

        float speed = Mathf.Lerp(0.01f, 0.06f, 1f - currentDialogue.talkingSpeed);

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
            uiManager.ShowDialogueNode(currentLineNode, npcName, speed, OnTypewriterComplete);
        }
        else if (currentNode is StartQuestNode questNode)
        {
            if (questNode.questToStart != null)
                QuestManager.Instance.StartQuest(questNode.questToStart);

            currentLineNode = new DialogueLineNode { text = questNode.text };
            lineFullyRevealed = false;
            waitingForPlayerInput = true;
            uiManager.ShowDialogueNode(currentLineNode, npcName, speed, OnTypewriterComplete);
        }
        else if (currentNode is EventNode eventNode)
        {
            if (!string.IsNullOrEmpty(eventNode.eventID))
                FindFirstObjectByType<EventManager>()?.TriggerEvent(eventNode.eventID);

            currentLineNode = new DialogueLineNode { text = eventNode.text };
            uiManager.ShowDialogueNode(currentLineNode, npcName, speed, OnTypewriterComplete);
        }
    }

    private void OnTypewriterComplete()
    {
        lineFullyRevealed = true;

        if (currentDialogue != null && currentDialogue.autoAdvance)
        {
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay());
        }
    }

    private IEnumerator AutoAdvanceAfterDelay()
    {
        int visibleChars = uiManager.GetVisibleCharCount();
        float delay = visibleChars * currentDialogue.secondsPerCharacter;
        yield return new WaitForSeconds(delay);
        autoAdvanceCoroutine = null;

        string nextID = GetNextNodeIDForCurrentNode();
        AdvanceNode(nextID);
    }

    private string GetNextNodeIDForCurrentNode()
    {
        if (currentNode is DialogueLineNode lineNode) return lineNode.nextNodeID;
        if (currentNode is EventNode eventNode) return eventNode.nextNodeID;
        if (currentNode is StartQuestNode questNode) return questNode.nextNodeID;
        return null;
    }

    private void AdvanceNode(string nextNodeID)
    {
        if (string.IsNullOrEmpty(nextNodeID))
        {
            EndDialogue();
            return;
        }

        if (!nodeLookup.TryGetValue(nextNodeID, out currentNode))
        {
            Debug.LogWarning($"Node '{nextNodeID}' not found. Ending dialogue.");
            EndDialogue();
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
            EndDialogue();
            return;
        }

        if (!nodeLookup.TryGetValue(nextNodeID, out currentNode))
        {
            Debug.LogWarning($"Node '{nextNodeID}' not found. Ending dialogue.");
            EndDialogue();
            return;
        }

        ProcessNode();
    }

    void EndDialogue()
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        uiManager.EndDialogue();

        if (currentDialogue != null)
            PlayerState.Instance.MarkDialogueComplete(currentDialogue);

        var npc = sendingObject?.GetComponent<NPCDialogueHolder>();
        if (npc != null) npc.EndConversation();

        if (currentDialogue != null && !currentDialogue.autoAdvance)
            TempStateMachine.Instance.SetState(GameState.Moving);

        currentNode = null;
        if (dialogueQueue.Count > 0)
        {
            var next = dialogueQueue.Dequeue();
            StartDialogue(next.dialogue, next.sender);
        }
        else
        {
            isDialogueActive = false;
        }
    }
}