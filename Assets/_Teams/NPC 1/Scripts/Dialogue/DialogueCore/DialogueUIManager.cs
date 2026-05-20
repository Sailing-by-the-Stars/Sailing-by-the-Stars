using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

// Created by Jantina
public class ShakeData
{
    public int startIndex;
    public int length;
}
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
    [SerializeField] private Image vignette;
    private Coroutine typewriterCoroutine;
    private Action onTypewriterComplete;
    private string currentFullText;
    private Coroutine vignetteCoroutine;
    private List<ShakeData> currentShakeData;
    private bool fastForwardHeld;
    private Dictionary<string, string> colorMap = new Dictionary<string, string>()
    {
        { "red", "#ff4d4d" },
        { "green", "#4dff4d" },
        { "blue", "#4da6ff" },
        { "yellow", "#ffff66" },
        { "purple", "#b84dff" },
        { "orange", "#ff944d" },
        { "pink", "#ff66cc" }
    };

    [Header("Audio")]
    [SerializeField] private EventReference defaultDialogueTypeSound;

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
        if (vignette != null)
        {
            Color c = vignette.color;
            vignette.color = new Color(c.r, c.g, c.b, 0f);
        }
    }
    private void Update()
    {
        if (dialogueBox.activeSelf)
            ApplyShake();
    }
    public void TriggerChoice1()
    {
        if (choiceButton1.gameObject.activeSelf)
            choiceButton1.onClick.Invoke();
    }

    public void TriggerChoice2()
    {
        if (choiceButton2.gameObject.activeSelf)
            choiceButton2.onClick.Invoke();
    }
    public void SetFastForward(bool held)
    {
        fastForwardHeld = held;
    }
    private void LateUpdate()
    {
        if (currentShakeData == null || currentShakeData.Count == 0) return;

        dialogueText.ForceMeshUpdate(true);
        var textInfo = dialogueText.textInfo;

        for (int i = 0; i < currentShakeData.Count; i++)
        {
            var shake = currentShakeData[i];

            for (int c = 0; c < shake.length; c++)
            {
                int charIndex = shake.startIndex + c;

                if (charIndex >= textInfo.characterCount) continue;
                if (!textInfo.characterInfo[charIndex].isVisible) continue;

                var charInfo = textInfo.characterInfo[charIndex];
                int vertexIndex = charInfo.vertexIndex;
                int matIndex = charInfo.materialReferenceIndex;

                var vertices = textInfo.meshInfo[matIndex].vertices;

                float offset = Mathf.Sin(Time.time * 20f + charIndex) * 2f;

                Vector3 shakeOffset = new Vector3(0, offset, 0);

                vertices[vertexIndex + 0] += shakeOffset;
                vertices[vertexIndex + 1] += shakeOffset;
                vertices[vertexIndex + 2] += shakeOffset;
                vertices[vertexIndex + 3] += shakeOffset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            dialogueText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    /// <summary>
    /// Displays a dialogue line node with typewriter effect.
    /// </summary>
    public void ShowDialogueNode(DialogueLineNode node, string npcName, float typingSpeed, Action typewriterCallback)
    {
        currentShakeData = null;
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
        
        bool hasAngry;
        List<ShakeData> shakes;

        currentFullText = ProcessText(node.text, out shakes, out hasAngry);

        currentShakeData = shakes;

        dialogueText.text = currentFullText;
        dialogueText.ForceMeshUpdate();
        ApplyAngryEffect(hasAngry);
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
        currentShakeData = null;
        bool hasAngry;
        List<ShakeData> shakes;
        currentFullText = ProcessText(choiceNode.text, out shakes, out hasAngry);
        currentShakeData = shakes;

        ApplyAngryEffect(hasAngry);
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
    private string ProcessText(string input, out List<ShakeData> shakeData, out bool hasAngry)
    {
        shakeData = new List<ShakeData>();
        hasAngry = false;
        var result = new System.Text.StringBuilder();
        int visibleCharCount = 0; // track only visible (non-tag) characters

        int i = 0;

        while (i < input.Length)
        {
            bool StartsWith(string val) => i + val.Length <= input.Length && input.Substring(i, val.Length) == val;

            if (StartsWith("<angry>")) { hasAngry = true; i += 7; continue; }
            if (StartsWith("</angry>")) { i += 8; continue; }
            if (StartsWith("<pause="))
            {
                int end = input.IndexOf('>', i);
                if (end != -1)
                {
                    result.Append(input.Substring(i, end - i + 1)); // pass through as-is for typewriter
                    i = end + 1;
                    continue; // no visible chars added
                }
            }
            if (StartsWith("<shake>"))
            {
                int startIndex = visibleCharCount;
                i += 7;
                int end = input.IndexOf("</shake>", i);
                if (end == -1) { Debug.LogWarning("Missing </shake> tag"); break; }

                string rawContent = input.Substring(i, end - i);

                // Process the inner content so <red> etc. get converted too
                bool innerAngry;
                List<ShakeData> innerShakes; // discard — nested shake not supported
                string processedContent = ProcessText(rawContent, out innerShakes, out innerAngry);
                if (innerAngry) hasAngry = true;

                // Count only visible characters in the processed content
                int visibleInContent = CountVisibleChars(processedContent);

                result.Append(processedContent);
                visibleCharCount += visibleInContent;

                shakeData.Add(new ShakeData { startIndex = startIndex, length = visibleInContent });
                i = end + 8;
                continue;
            }

            bool matchedColor = false;
            foreach (var kvp in colorMap)
            {
                string openTag = $"<{kvp.Key}>", closeTag = $"</{kvp.Key}>";
                if (StartsWith(openTag))  { result.Append($"<color={kvp.Value}>"); i += openTag.Length; matchedColor = true; break; }
                if (StartsWith(closeTag)) { result.Append("</color>"); i += closeTag.Length; matchedColor = true; break; }
            }
            if (matchedColor) continue; // color tags add no visible chars, don't increment

            result.Append(input[i]);
            visibleCharCount++; // normal visible character
            i++;
        }

        return result.ToString();
    }
    private int CountVisibleChars(string processedText)
    {
        int count = 0;
        int i = 0;
        while (i < processedText.Length)
        {
            if (processedText[i] == '<')
            {
                int end = processedText.IndexOf('>', i);
                if (end != -1) { i = end + 1; continue; } // skip tag
            }
            count++;
            i++;
        }
        return count;
    }
    private void ApplyAngryEffect(bool active)
    {
        if (vignetteCoroutine != null)
            StopCoroutine(vignetteCoroutine);

        vignetteCoroutine = StartCoroutine(FadeVignette(active ? 1f : 0f));
    }

    private IEnumerator FadeVignette(float targetAlpha)
    {
        Color color = vignette.color;
        float startAlpha = color.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            vignette.color = new Color(color.r, color.g, color.b, newAlpha);

            yield return null;
        }

        vignetteCoroutine = null;
    }

    /// <summary>
    /// Coroutine for typing out dialogue character by character.
    /// </summary>
    private IEnumerator TypewriterEffect(string fullText, float speed)
    {
        dialogueText.text = "";
        dialogueText.ForceMeshUpdate();

        for (int i = 0; i < fullText.Length; i++)
        {
            // Handle tags (including pause)
            if (fullText[i] == '<')
            {
                int endIndex = fullText.IndexOf('>', i);
                if (endIndex != -1)
                {
                    string tag = fullText.Substring(i + 1, endIndex - i - 1);

                    // Handle pause tag
                    if (tag.StartsWith("pause="))
                    {
                        string value = tag.Replace("pause=", "");
                        if (float.TryParse(value, out float pauseTime))
                        {
                            yield return new WaitForSeconds(pauseTime);
                        }

                        i = endIndex;
                        continue;
                    }

                    // Append rich text tags (color etc.)
                    dialogueText.text += fullText.Substring(i, endIndex - i + 1);
                    i = endIndex;
                    continue;
                }
            }

            dialogueText.text += fullText[i];
            dialogueText.ForceMeshUpdate();
            ApplyShake();

            if (!char.IsWhiteSpace(fullText[i]) &&
                !char.IsPunctuation(fullText[i]))
            {
                var npcVoice = DialogueSystem.Instance.CurrentVoice;

                if (!npcVoice.IsNull)
                {
                    RuntimeManager.PlayOneShot(npcVoice);
                }

                else if (!defaultDialogueTypeSound.IsNull)
                {
                    RuntimeManager.PlayOneShot(defaultDialogueTypeSound);
                }
            }

            float multiplier = fastForwardHeld ? 5f : 1f;
            yield return new WaitForSeconds(speed / multiplier);
        }

        typewriterCoroutine = null;
        onTypewriterComplete?.Invoke();
    }
    private void ApplyShake()
    {
        if (currentShakeData == null || currentShakeData.Count == 0) return;

        dialogueText.ForceMeshUpdate();
        var textInfo = dialogueText.textInfo;

        for (int i = 0; i < currentShakeData.Count; i++)
        {
            var shake = currentShakeData[i];

            for (int c = 0; c < shake.length; c++)
            {
                int charIndex = shake.startIndex + c;

                if (charIndex >= textInfo.characterCount) continue;
                if (!textInfo.characterInfo[charIndex].isVisible) continue;

                var charInfo = textInfo.characterInfo[charIndex];
                int vertexIndex = charInfo.vertexIndex;
                int matIndex = charInfo.materialReferenceIndex;

                var vertices = textInfo.meshInfo[matIndex].vertices;

                float offset = Mathf.Sin(Time.time * 20f + charIndex) * 2f;
                Vector3 shakeOffset = new Vector3(0, offset, 0);

                vertices[vertexIndex + 0] += shakeOffset;
                vertices[vertexIndex + 1] += shakeOffset;
                vertices[vertexIndex + 2] += shakeOffset;
                vertices[vertexIndex + 3] += shakeOffset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            dialogueText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
    private string StripPauseTags(string input)
    {
        var result = new System.Text.StringBuilder();
        int i = 0;
        while (i < input.Length)
        {
            if (input[i] == '<')
            {
                int end = input.IndexOf('>', i);
                if (end != -1)
                {
                    string tag = input.Substring(i + 1, end - i - 1);
                    if (tag.StartsWith("pause=")) { i = end + 1; continue; }
                    result.Append(input, i, end - i + 1);
                    i = end + 1;
                    continue;
                }
            }
            result.Append(input[i]);
            i++;
        }
        return result.ToString();
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
            dialogueText.text = StripPauseTags(currentFullText);
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
        ApplyAngryEffect(false);
        DialogueSystem.Instance.isDialogueActive = false;
    }
}