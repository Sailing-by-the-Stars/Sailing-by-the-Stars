using TMPro;
using UnityEngine;

public class MorseUIText : MonoBehaviour
{
    [SerializeField] private MorseDecoder morseDecoder;
    [SerializeField] private TextMeshProUGUI morseText;

    private void OnEnable()
    {
        morseDecoder.OnMorseProgressUpdated += UpdateText;
        morseDecoder.OnMorseReset += ResetText;
    }

    private void OnDisable()
    {
        morseDecoder.OnMorseProgressUpdated -= UpdateText;
        morseDecoder.OnMorseReset -= ResetText;
    }

    private void Start()
    {
        ResetText();
    }

    private void UpdateText(string current)
    {
        morseText.text = current;
    }

    private void ResetText()
    {
        morseText.text = "";
    }
}