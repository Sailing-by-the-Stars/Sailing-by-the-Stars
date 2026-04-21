using UnityEngine;

/// <summary>
/// TESTING ONLY — remove before shipping.
///
/// Marks all three island puzzles as complete so the Acceptance Island
/// pedestals become immediately interactable without running the full game.
///
/// SETUP:
///   Attach to any GameObject in the scene (e.g. the PedestalSystem).
///   Press the chosen key at runtime to unlock all pedestals at once,
///   or tick "Unlock On Start" to unlock them automatically when the
///   scene loads.
/// </summary>
public class DebugPedestalUnlocker : MonoBehaviour
{
    [Tooltip("Key to press at runtime to unlock all pedestals.")]
    public KeyCode unlockKey = KeyCode.F8;

    [Tooltip("If true, all pedestals are unlocked as soon as the scene starts.")]
    public bool unlockOnStart = false;

    private void Start()
    {
        if (unlockOnStart)
            UnlockAll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(unlockKey))
            UnlockAll();
    }

    private void UnlockAll()
    {
        PuzzleProgress.MarkComplete("anger");
        PuzzleProgress.MarkComplete("bargaining");
        PuzzleProgress.MarkComplete("depression");
        Debug.Log("[DebugPedestalUnlocker] All three puzzles marked complete — " +
                  "walk up to each pedestal and press E.");
    }
}
