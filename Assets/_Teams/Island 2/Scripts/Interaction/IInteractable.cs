// Author: Edward
public interface IInteractable
{
    // Text that shows up in the Canvas when aiming at an Interactable
    public string InteractMessage { get; }

    public bool ShouldShowMessage(InteractionController interactionController) { return true; }
    
    // Choose: if your Interactable needs to have a "hold button down" functionality, override HoldInteract and optionally ReleaseInteract
    // Otherwise, override just Interact
    // DON'T USE BOTH OPTIONS, IT WILL CAUSE BUGS.
    public void Interact(InteractionController interactionController) {}
    public void HoldInteract(InteractionController interactionController) {}
    public void ReleaseInteract(InteractionController interactionController) {}
}