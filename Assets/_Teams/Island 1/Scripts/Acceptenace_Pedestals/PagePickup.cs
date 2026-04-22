using UnityEngine;

/// <summary>
/// Optional physical collectible that marks a puzzle as complete when
/// the player picks it up. Place one of these at each island's puzzle
/// reward point, disabled by default, then enable it on puzzle completion.
///
/// INSPECTOR SETUP:
///   Page ID  — must match the pedestal's pageID exactly:
///              "anger"      → Pedestal_Anger
///              "bargaining" → Pedestal_Bargaining
///              "depression" → Pedestal_Depression
///
/// HOW TO CONNECT TO PUZZLE COMPLETION (choose one):
///   Option A — Start the GameObject disabled. In your puzzle's completion
///              code call: gameObject.SetActive(true).
///   Option B — Wire gameObject.SetActive(true) to your puzzle's UnityEvent.
///   Option C — Skip the physical pickup entirely and call the static helper:
///              PagePickup.Award("anger");
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PagePickup : PhysicsPickup
{
    [Header("Puzzle Page")]
    [Tooltip("Must exactly match the pedestal's pageID: 'anger', 'bargaining', or 'depression'")]
    [SerializeField] private string pageID;

    public override string InteractMessage => $"Press E to collect the {pageID} page";
    
    /// <summary>
    /// Consumed immediately on pickup — not held in hand.
    /// </summary>
    public override void Grab(PickupController pickupController)
    {
        Award(pageID);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Awards puzzle completion without requiring a physical pickup.
    /// Call directly from any puzzle-completion script:
    ///   PagePickup.Award("anger");
    /// </summary>
    public static void Award(string puzzleID)
    {
        PuzzleProgress.MarkComplete(puzzleID);
    }
}
