// Programmer: Arch

using UnityEngine;

/// <summary>
/// Looping sparkle particle controlled by a puzzle hint manager.
/// Call ShowSparkle() and HideSparkle() to turn the effect on and off.
/// </summary>
public class GuidingSparklePoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem sparkleParticles;

    [Header("Settings")]
    [SerializeField] private bool startVisible = false;

    private void Awake()
    {
        if (startVisible)
            ShowSparkle();
        else
            HideSparkle();
    }

    /// <summary>Starts the sparkle particle effect on this point.</summary>
    public void ShowSparkle()
    {
        if (sparkleParticles.isPlaying) return;
        sparkleParticles.Clear();
        sparkleParticles.Play();
    }

    /// <summary>Stops the sparkle particle effect on this point.</summary>
    public void HideSparkle()
    {
        sparkleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
