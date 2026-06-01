using UnityEngine;

public class MorseButton : MonoBehaviour, IInteractable
{
    [SerializeField] private MorseDecoder morseDecoder;
    private MorseAudio morseAudio;
    
    [SerializeField] private string objectInteractMessage;
    public string InteractMessage => objectInteractMessage;
    public bool ShouldShowMessage => true;

    private void Start()
    {
        morseAudio = morseDecoder.GetComponent<MorseAudio>();
    }

    public void HoldInteract(InteractionController interactionController)
    {
        morseDecoder.BeginSignal();
        morseAudio.StartBeep();
    }

    public void ReleaseInteract(InteractionController interactionController)
    {
        morseDecoder.EndSignal();
        morseAudio.StopBeep();
    }
}