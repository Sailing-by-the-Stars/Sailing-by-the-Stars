using UnityEngine;
using TMPro;

/// <summary>
/// Controls the interaction prompt shown when near an NPC.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance;

    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;

    void Awake()
    {
        Instance = this;
        HidePrompt();
    }

    public void ShowPrompt(string message)
    {
        promptObject.SetActive(true);
        promptText.text = message;
    }

    public void HidePrompt()
    {
        promptObject.SetActive(false);
    }
}