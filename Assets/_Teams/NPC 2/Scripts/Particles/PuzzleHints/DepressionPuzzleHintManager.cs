// Programmer: Arch

using UnityEngine;

/// <summary>
/// Controls the Morse guiding light for the Depression puzzle.
/// Stops the pattern once the player enters their first correct letter,
/// and restarts it if the player resets before then.
/// </summary>
public class DepressionPuzzleHintManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuidingMorseLight morseLight;
    [SerializeField] private MorseDecoder morseDecoder;

    private bool firstLetterEntered = false;

    private void OnEnable()
    {
        morseDecoder.OnMorseProgressUpdated += HandleProgressUpdated;
        morseDecoder.OnMorseReset += HandleMorseReset;
        morseDecoder.OnMorseSolved += HandleMorseSolved;
    }

    private void OnDisable()
    {
        morseDecoder.OnMorseProgressUpdated -= HandleProgressUpdated;
        morseDecoder.OnMorseReset -= HandleMorseReset;
        morseDecoder.OnMorseSolved -= HandleMorseSolved;
    }

    private void HandleProgressUpdated(string progress)
    {
        if (firstLetterEntered) return;
        if (string.IsNullOrEmpty(progress)) return;

        firstLetterEntered = true;
        morseLight.StopMorsePattern();
    }

    private void HandleMorseReset()
    {
        if (firstLetterEntered) return;

        morseLight.StopMorsePattern();
        morseLight.StartMorsePattern();
    }

    private void HandleMorseSolved()
    {
        morseLight.StopMorsePattern();
    }
}
