using UnityEngine;

public class MorseButton : MonoBehaviour, IInteractable
{
    [SerializeField] private MorseDecoder morseDecoder;
    [SerializeField] private string objectInteractMessage;
    
    public string InteractMessage => objectInteractMessage;
    public bool ShouldShowMessage => true;
    
    public void HoldInteract(InteractionController interactionController)
    {
        morseDecoder.BeginSignal();
    }

    public void ReleaseInteract(InteractionController interactionController)
    {
        morseDecoder.EndSignal();
    }
}