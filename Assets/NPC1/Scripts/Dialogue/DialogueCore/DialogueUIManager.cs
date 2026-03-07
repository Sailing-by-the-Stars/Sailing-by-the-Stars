using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
// Created by Jantina
public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Button choiceButton2;

    private Coroutine typewriterCoroutine;
    private Action onTypewriterComplete;
    private string currentFullText;

    public bool TypewriterRunning => typewriterCoroutine != null;

    private void Awake()
    {
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.gameObject.SetActive(false);
    }

    public void ShowDialogueNode(DialogueLineNode node, string npcName, float typingSpeed, Action typewriterCallback)
    {
        npcNameText.text = npcName;
        dialogueText.text = "";
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.gameObject.SetActive(true);

        currentFullText = node.text;
        onTypewriterComplete = () =>
        {
            nextButton.gameObject.SetActive(true);
            typewriterCallback?.Invoke();
        };

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentFullText, typingSpeed));
    }

    public void ShowChoiceNode(ChoiceNode choiceNode, string npcName, Action<string> onChoiceSelected, float typingSpeed)
    {
        npcNameText.text = npcName;
        dialogueText.text = "";
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.gameObject.SetActive(true);

        currentFullText = choiceNode.text;
        onTypewriterComplete = () =>
        {
            SetupChoiceButtons(choiceNode, onChoiceSelected);
        };

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentFullText, typingSpeed));
    }

    private void SetupChoiceButtons(ChoiceNode choiceNode, Action<string> onChoiceSelected)
    {
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);

        if (choiceNode.choices.Count > 0)
        {
            choiceButton1.gameObject.SetActive(true);
            TMP_Text txt1 = choiceButton1.GetComponentInChildren<TMP_Text>();
            txt1.text = choiceNode.choices[0].choiceText;
            choiceButton1.onClick.RemoveAllListeners();
            string nextID1 = choiceNode.choices[0].nextNodeID;
            choiceButton1.onClick.AddListener(() => onChoiceSelected(nextID1));
        }

        if (choiceNode.choices.Count > 1)
        {
            choiceButton2.gameObject.SetActive(true);
            TMP_Text txt2 = choiceButton2.GetComponentInChildren<TMP_Text>();
            txt2.text = choiceNode.choices[1].choiceText;
            choiceButton2.onClick.RemoveAllListeners();
            string nextID2 = choiceNode.choices[1].nextNodeID;
            choiceButton2.onClick.AddListener(() => onChoiceSelected(nextID2));
        }
    }

    private IEnumerator TypewriterEffect(string fullText, float speed)
    {
        dialogueText.text = "";
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speed);
        }

        typewriterCoroutine = null;
        onTypewriterComplete?.Invoke();
    }

    public void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;

            dialogueText.text = currentFullText;
            onTypewriterComplete?.Invoke();
        }
    }

    public void EndDialogue()
    {
        dialogueText.text = "";
        npcNameText.text = "";
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.gameObject.SetActive(false);
    }
}