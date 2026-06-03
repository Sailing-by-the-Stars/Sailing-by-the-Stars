// Programmer: Arch

using System.Collections;
using UnityEngine;

/// <summary>
/// Plays the guiding sparkle sequence for the Anger (Shadow) puzzle.
/// On entering the puzzle area the statue sparkle appears; when the player
/// reaches the statues the wall sparkles show, then the button sparkles in order.
/// Attach to a GameObject with a trigger collider at the puzzle area entrance.
/// </summary>
public class AngerPuzzleHintManager : MonoBehaviour
{
    [Header("Statue Hint")]
    [SerializeField] private GuidingSparklePoint statueSparkle;

    [Header("Wall Hints")]
    [SerializeField] private GuidingSparklePoint[] wallSparkles;

    [Header("Button Hints (in press order)")]
    [SerializeField] private GuidingSparklePoint[] buttonSparkles;

    [Header("Timing (seconds)")]
    [SerializeField] private float wallDisplayDuration = 6f;
    [SerializeField] private float buttonDisplayInterval = 3f;

    private bool sequenceStarted = false;
    private bool reachedStatues = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (sequenceStarted) return;

        sequenceStarted = true;
        statueSparkle.ShowSparkle();
    }

    /// <summary>
    /// Hides the statue sparkle and plays the wall + button sequence.
    /// Called by AngerStatueReachTrigger when the player reaches the statues.
    /// </summary>
    public void ReachedStatues()
    {
        if (!sequenceStarted || reachedStatues) return;

        reachedStatues = true;
        statueSparkle.HideSparkle();
        StartCoroutine(PlayWallAndButtonSequence());
    }

    private IEnumerator PlayWallAndButtonSequence()
    {
        ShowAllSparkles(wallSparkles);
        yield return new WaitForSeconds(wallDisplayDuration);
        HideAllSparkles(wallSparkles);

        foreach (GuidingSparklePoint button in buttonSparkles)
        {
            button.ShowSparkle();
            yield return new WaitForSeconds(buttonDisplayInterval);
            button.HideSparkle();
        }
    }

    private void ShowAllSparkles(GuidingSparklePoint[] sparkles)
    {
        foreach (GuidingSparklePoint sparkle in sparkles)
            sparkle.ShowSparkle();
    }

    private void HideAllSparkles(GuidingSparklePoint[] sparkles)
    {
        foreach (GuidingSparklePoint sparkle in sparkles)
            sparkle.HideSparkle();
    }
}
