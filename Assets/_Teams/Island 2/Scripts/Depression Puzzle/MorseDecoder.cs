using System;
using System.Collections.Generic;
using UnityEngine;

public class MorseDecoder : MonoBehaviour
{
    [SerializeField] private GameObject rewardItem;
    
    [Header("Target")]
    [SerializeField] private string targetSequence = "OPENTHEDOOR";

    [Header("Timing (seconds)")]
    [Tooltip("How long should it be pressed to be considered a dash.")]
    [SerializeField, Min(0.05f)] private float dotDashThreshold = 0.25f;
    [Tooltip("How much time should pass for the next input to be considered part of a new letter.")]
    [SerializeField, Min(0.05f)] private float letterGapThreshold = 0.45f;

    public event Action OnMorseSolved;
    public event Action<string> OnMorseProgressUpdated;
    public event Action OnMorseReset;

    private readonly Dictionary<string, char> morseMap = new()
    {
        { ".-", 'A' },
        { "-...", 'B' },
        { "-.-.", 'C' },
        { "-..", 'D' },
        { ".", 'E' },
        { "..-.", 'F' },
        { "--.", 'G' },
        { "....", 'H' },
        { "..", 'I' },
        { ".---", 'J' },
        { "-.-", 'K' },
        { ".-..", 'L' },
        { "--", 'M' },
        { "-.", 'N' },
        { "---", 'O' },
        { ".--.", 'P' },
        { "--.-", 'Q' },
        { ".-.", 'R' },
        { "...", 'S' },
        { "-", 'T' },
        { "..-", 'U' },
        { "...-", 'V' },
        { ".--", 'W' },
        { "-..-", 'X' },
        { "-.--", 'Y' },
        { "--..", 'Z' }
    };

    private string normalizedTarget;
    private int progressIndex;

    private string currentLetterMorse = "";
    private bool isPressing;
    private float pressStartTime;
    private float lastReleaseTime = -999f;
    private bool solved;

    private void Awake()
    {
        normalizedTarget = NormalizeTarget(targetSequence);
        ResetProgressInternal();
    }

    private void Update()
    {
        if (solved || isPressing) return;

        if (!string.IsNullOrEmpty(currentLetterMorse) && Time.time - lastReleaseTime >= letterGapThreshold)
        {
            FinalizeCurrentLetter();
        }
    }

    public void BeginSignal()
    {
        if (solved) return;

        // If the player starts the next input after a valid pause,
        // finalize the previous letter before starting the new one.
        if (!isPressing && !string.IsNullOrEmpty(currentLetterMorse) && Time.time - lastReleaseTime >= letterGapThreshold)
        {
            FinalizeCurrentLetter();

            if (solved) return;
        }

        isPressing = true;
        pressStartTime = Time.time;
    }

    public void EndSignal()
    {
        if (solved || !isPressing) return;

        isPressing = false;

        float heldFor = Time.time - pressStartTime;
        currentLetterMorse += heldFor <= dotDashThreshold ? "." : "-";
        lastReleaseTime = Time.time;
    }

    private void FinalizeCurrentLetter()
    {
        if (string.IsNullOrEmpty(currentLetterMorse)) return;

        if (!morseMap.TryGetValue(currentLetterMorse, out char decoded))
        {
            Debug.LogWarning("Invalid Letter.");
            ResetProgress();
            return;
        }
        Debug.Log("Letter: " + decoded);
        if (progressIndex >= normalizedTarget.Length)
        {
            ResetProgress();
            return;
        }

        char expected = normalizedTarget[progressIndex];

        if (char.ToUpperInvariant(decoded) != expected)
        {
            Debug.LogWarning("Wrong Letter.");
            ResetProgress();
            return;
        }

        progressIndex++;
        currentLetterMorse = "";
        OnMorseProgressUpdated?.Invoke(GetCurrentProgress());

        if (progressIndex < normalizedTarget.Length) return;
        
        solved = true;
        OnMorseSolved?.Invoke();
        TestSuccess();
    }

    public void ResetProgress()
    {
        ResetProgressInternal();
        OnMorseProgressUpdated?.Invoke("");
        OnMorseReset?.Invoke();
    }

    private void ResetProgressInternal()
    {
        progressIndex = 0;
        currentLetterMorse = "";
        isPressing = false;
        pressStartTime = 0f;
        lastReleaseTime = -999f;
        solved = false;
    }

    private string NormalizeTarget(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim().ToUpperInvariant().Replace(" ", "");
    }

    private void TestSuccess()
    {
        Debug.Log("Success!");
        if (rewardItem)
        {
            rewardItem.SetActive(true);
        }
    }

    public string GetCurrentProgress()
    {
        if (string.IsNullOrEmpty(normalizedTarget) || progressIndex <= 0)
            return string.Empty;

        return normalizedTarget.Substring(0, progressIndex);
    }
}