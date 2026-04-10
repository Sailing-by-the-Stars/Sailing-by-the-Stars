// Author: Edward
public interface IInteractable
{
    // Text that shows up in the Canvas when aiming at an Interactable
    public string InteractMessage { get; }

    public bool ShouldShowMessage(InteractionController interactionController) { return true; }
    public void Interact(InteractionController interactionController);
}