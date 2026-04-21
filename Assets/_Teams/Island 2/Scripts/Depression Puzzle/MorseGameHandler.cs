using UnityEngine;

public class MorseGameHandler : MonoBehaviour
{
    [SerializeField] private MorseBuffer buffer;

    private void OnEnable()
    {
        buffer.OnSequenceSubmitted += HandleDecodedInput;
    }

    private void OnDisable()
    {
        buffer.OnSequenceSubmitted -= HandleDecodedInput;
    }

    private void HandleDecodedInput(string decoded)
    {
        if (string.IsNullOrEmpty(decoded))
        {
            Debug.Log("Invalid code entered.");
            return;
        }
        
        Debug.Log("Player entered: " + decoded);

        if (decoded == "A")
        {
            Debug.Log("Trigger action A");
        }
        else if (decoded == "B")
        {
            Debug.Log("Trigger action B");
        }
    }
}