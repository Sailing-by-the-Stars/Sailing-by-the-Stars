// Programmer: Arch

using UnityEngine;

/// <summary>
/// Trigger placed at the statues. When the player enters it, tells the
/// AngerPuzzleHintManager that the player has reached the statues.
/// </summary>
public class AngerStatueReachTrigger : MonoBehaviour
{
    [SerializeField] private AngerPuzzleHintManager hintManager;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        hintManager.ReachedStatues();
    }
}
