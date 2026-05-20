using UnityEngine;

/// <summary>
/// Attach to each pedestal GameObject on the Acceptance Island staircase.
///
/// INSPECTOR SETUP:
///   Page ID          — must match the puzzleID used when the island calls
///                      PuzzleProgress.MarkComplete(...):
///                      "anger", "bargaining", or "depression"
///   Item Label       — display name shown in the prompt, e.g. "Anger"
///   Pedestal Manager — drag the PedestalSystem GameObject here
///   Filled Material  — material applied to the pedestal top when filled (optional)
///   Pedestal Renderer — renderer on the pedestal top (optional)
///   Item Display Model — child object shown on top when filled (optional)
///
/// </summary>
public class ItemPedestal : MonoBehaviour, IInteractable
{
    [Header("Puzzle Integration")]
    [Tooltip("Must match the puzzleID passed to PuzzleProgress.MarkComplete(): 'anger', 'bargaining', or 'depression'")]
    public string pageID;

    [Tooltip("Name shown in the interaction prompt, e.g. \"Anger\"")]
    public string itemLabel = "Item";

    [Header("References")]
    public AcceptanceIslandManager pedestalManager;

    [Header("Visuals (optional)")]
    public GameObject itemDisplayModel;
    public Renderer pedestalRenderer;
    public Material filledMaterial;

    public bool IsFilled { get; private set; }
    
    /// <summary>
    /// Text shown by InteractionController when the player looks at this pedestal.
    /// </summary>
    public string InteractMessage => IsPuzzleComplete()
        ? $"Press E to place {itemLabel}"
        : "Complete all island puzzles first";

    /// <summary>
    /// Hide the prompt once the pedestal is already filled.
    /// </summary>
    public bool ShouldShowMessage(InteractionController interactionController) => !IsFilled;

    /// <summary>
    /// Called by InteractionController when the player presses E.
    /// </summary>
    public void Interact(InteractionController interactionController)
    {
        if (IsFilled || !IsPuzzleComplete()) return;
        Fill();
    }
    
    private void Start()
    {
        if (itemDisplayModel != null)
            itemDisplayModel.SetActive(false);
    }
    
    private bool IsPuzzleComplete() => pedestalManager != null
        ? pedestalManager.AreAllPuzzlesComplete()
        : PuzzleProgress.IsComplete(pageID);

    private void Fill()
    {
        IsFilled = true;

        if (itemDisplayModel != null)
            itemDisplayModel.SetActive(true);

        if (pedestalRenderer != null && filledMaterial != null)
            pedestalRenderer.material = filledMaterial;

        pedestalManager?.OnPedestalFilled(this);

        Debug.Log($"[ItemPedestal] '{itemLabel}' placed on '{name}'.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsFilled ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}
