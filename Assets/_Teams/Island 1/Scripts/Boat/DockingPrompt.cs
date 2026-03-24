using UnityEngine;
using TMPro;

public class DockingPrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Settings")]
    [SerializeField] private string promptMessage = "Press F to Dock";

    private void Start()
    {
        promptText.text = promptMessage;
        promptObject.SetActive(false);
    }

    /// <summary>Show the prompt with a custom message.</summary>
    public void ShowPrompt(string message)
    {
        promptText.text = message;
        promptObject.SetActive(true);
    }

    /// <summary>Show the prompt with the default message set in the Inspector.</summary>
    public void ShowPrompt()
    {
        promptText.text = promptMessage;
        promptObject.SetActive(true);
    }

    public void HidePrompt()
    {
        promptObject.SetActive(false);
    }
}