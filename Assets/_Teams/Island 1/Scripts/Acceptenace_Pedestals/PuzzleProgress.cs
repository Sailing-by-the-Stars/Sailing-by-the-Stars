using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight static registry that tracks which island puzzles have been completed.
///
/// HOW TO USE FROM OTHER ISLAND SCRIPTS:
///   When a puzzle is finished, call:
///       PuzzleProgress.MarkComplete("anger");      // Anger island
///       PuzzleProgress.MarkComplete("bargaining"); // Bargaining island
///       PuzzleProgress.MarkComplete("depression"); // Depression island
///
///   The corresponding Acceptance Island pedestal will become interactable
///   the next time the player approaches it.
/// </summary>
public static class PuzzleProgress
{
    private static readonly HashSet<string> _completed = new HashSet<string>();

    /// <summary>
    /// Mark a puzzle as completed by its ID.
    /// Safe to call multiple times — duplicates are ignored.
    /// </summary>
    public static void MarkComplete(string puzzleID)
    {
        if (string.IsNullOrEmpty(puzzleID)) return;

        if (_completed.Add(puzzleID))
            Debug.Log($"[PuzzleProgress] Puzzle '{puzzleID}' marked complete.");
        else
            Debug.Log($"[PuzzleProgress] Puzzle '{puzzleID}' was already complete.");
    }

    /// <summary>
    /// Returns true if the puzzle with the given ID has been completed.
    /// </summary>
    public static bool IsComplete(string puzzleID) =>
        !string.IsNullOrEmpty(puzzleID) && _completed.Contains(puzzleID);

    /// <summary>
    /// Clears all recorded progress. Call on new game / game reset.
    /// </summary>
    public static void Reset()
    {
        _completed.Clear();
        Debug.Log("[PuzzleProgress] Progress reset.");
    }
}
