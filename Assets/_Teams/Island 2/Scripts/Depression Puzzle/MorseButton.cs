using UnityEngine;

public class MorseButton : MonoBehaviour, IInteractable
{
    [SerializeField] private MorseBuffer buffer;
    [SerializeField] private bool isShort;
    [SerializeField] private string objectInteractMessage;
    
    public string InteractMessage => objectInteractMessage;
    public bool ShouldShowMessage => true;
    
    public void Interact(InteractionController interactionController)
    {
        InputMorse();
    }

    private void InputMorse()
    {
        if (isShort)
        {
            buffer.AddShortSignal();
        }
        else
        {
            buffer.AddLongSignal();
        }
    }
}