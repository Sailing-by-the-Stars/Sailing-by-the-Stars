using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
// Created by Jantina

public class DialogueUIManager : MonoBehaviour
{

    

    [Tooltip("The main dialogue panel that contains all dialogue UI elements.")]
    [SerializeField] private GameObject dialogueBox;

    [Tooltip("Nameplate for the npcs")]
    [SerializeField] private GameObject npcNameplate;

    [Tooltip("Text component displaying the NPC's name.")]
    [SerializeField] private TMP_Text npcNameText;

    [Tooltip("Text component displaying the dialogue text.")]
    [SerializeField] private TMP_Text dialogueText;

    [Tooltip("Button to advance to the next dialogue line.")]
    [SerializeField] private Button nextButton;

    [Tooltip("First choice button for player dialogue options.")]
    [SerializeField] private Button choiceButton1;

    [Tooltip("Second choice button for player dialogue options.")]
    [SerializeField] private Button choiceButton2;

    [Tooltip("Image background for dialogue options.")]
    [SerializeField] private Image choiceButtonImage;

    private Coroutine typewriterCoroutine;
    private Action onTypewriterComplete;
    private string currentFullText;

    /// <summary>
    /// Returns true if the typewriter effect is currently running.
    /// </summary>
    public bool TypewriterRunning => typewriterCoroutine != null;

    private void Awake()
    {
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (choiceButton1.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha1))
        {
            choiceButton1.onClick.Invoke();
        }

        if (choiceButton2.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha2))
        {
            choiceButton2.onClick.Invoke();
        }
    }

    /// <summary>
    /// Displays a dialogue line node with typewriter effect.
    /// </summary>
    public void ShowDialogueNode(DialogueLineNode node, string npcName, float typingSpeed, Action typewriterCallback)
    {
        npcNameText.text = npcName;

        if (npcName == "" )
        {
            npcNameplate.SetActive(false);

        }
        else
        {
            npcNameplate.SetActive(true);
        }

        dialogueText.text = "";
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.SetActive(true);
        choiceButtonImage.enabled=false;
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

    /// <summary>
    /// Displays a choice node with typewriter effect, then activates choice buttons when done.
    /// </summary>
    public void ShowChoiceNode(ChoiceNode choiceNode, string npcName, Action<string> onChoiceSelected, float typingSpeed)
    {
        if (npcName == "" )
        {
            npcNameplate.SetActive(false);

        }
        else
        {
            npcNameplate.SetActive(true);
        }
        
        npcNameText.text = npcName;
        dialogueText.text = "";
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.SetActive(true);
    
        currentFullText = choiceNode.text;
        onTypewriterComplete = () =>
        {
            SetupChoiceButtons(choiceNode, onChoiceSelected);
        };

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentFullText, typingSpeed));
    }

    /// <summary>
    /// Activates choice buttons and assigns click events for player selection.
    /// </summary>
    private void SetupChoiceButtons(ChoiceNode choiceNode, Action<string> onChoiceSelected)
    {
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        choiceButtonImage.enabled=true;

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

    /// <summary>
    /// Coroutine for typing out dialogue character by character.
    /// </summary>
    private IEnumerator TypewriterEffect(string fullText, float speed)
    {
        dialogueText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            // Detect tags like <pause=1>
            if (fullText[i] == '<')
            {
                int endIndex = fullText.IndexOf('>', i);
                if (endIndex != -1)
                {
                    string tag = fullText.Substring(i + 1, endIndex - i - 1);

                    if (tag.StartsWith("pause="))
                    {
                        string value = tag.Replace("pause=", "");
                        if (float.TryParse(value, out float pauseTime))
                        {
                            yield return new WaitForSeconds(pauseTime);
                        }
                    }

                    i = endIndex;
                    continue;
                }
            }

            dialogueText.text += fullText[i];
            float multiplier = Input.GetMouseButton(0) ? 5f : 1f;
            yield return new WaitForSeconds(speed / multiplier);
        }

        typewriterCoroutine = null;
        onTypewriterComplete?.Invoke();
    }
    private string StripTags(string input)
    {
        string result = "";

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '<')
            {
                int endIndex = input.IndexOf('>', i);
                if (endIndex != -1)
                {
                    i = endIndex;
                    continue;
                }
            }

            result += input[i];
        }

        return result;
    }

    /// <summary>
    /// Skips the typewriter effect and instantly shows the full dialogue text.
    /// </summary>
    public void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            
            dialogueText.text = StripTags(currentFullText);
            onTypewriterComplete?.Invoke();
        }
    }

    /// <summary>
    /// Ends the dialogue and hides all UI elements.
    /// </summary>
    public void EndDialogue()
    {
        dialogueText.text = "";
        npcNameText.text = "";
        nextButton.gameObject.SetActive(false);
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        dialogueBox.SetActive(false);
        DialogueSystem.Instance.isDialogueActive = false;
    }
}