public interface IInteractable
{
    // Text that shows up in the Canvas when aiming at an Interactable
    public string InteractMessage { get; }
    
    public void Interact(InteractionController interactionController);
}