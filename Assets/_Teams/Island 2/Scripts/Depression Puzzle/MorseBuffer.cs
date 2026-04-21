using System;
using UnityEngine;

public class MorseBuffer : MonoBehaviour
{
    [SerializeField] private MorseDecoder decoder;
    [SerializeField] private float submitDelay = 1.5f;

    public event Action<string> OnSequenceSubmitted;

    private string currentSequence = "";
    private float lastInputTime = -999f;
    private bool hasInput = false;

    private void Update()
    {
        if (hasInput && Time.time - lastInputTime >= submitDelay)
        {
            SubmitSequence();
        }
    }

    public void AddShortSignal()
    {
        currentSequence += ".";
        lastInputTime = Time.time;
        hasInput = true;
        Debug.Log("Added short signal: " + currentSequence);
    }

    public void AddLongSignal()
    {
        currentSequence += "-";
        lastInputTime = Time.time;
        hasInput = true;
        Debug.Log("Added long signal: " + currentSequence);
    }

    private void SubmitSequence()
    {
        if (string.IsNullOrEmpty(currentSequence)) return;
        
        Debug.Log("Submitting Morse: " + currentSequence);

        if (decoder && decoder.TryDecode(currentSequence, out string decoded))
        {
            Debug.Log("Decoded to: " + decoded);
            OnSequenceSubmitted?.Invoke(decoded);
        }
        else
        {
            Debug.Log("Unknown Morse sequence.");
            OnSequenceSubmitted?.Invoke("");
        }
        
        currentSequence = "";
        hasInput = false;
    }
}