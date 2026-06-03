// Programmer: Arch

using UnityEngine;

/// <summary>
/// Sparkle particle that disappears when the player walks through it.
/// Requires a ParticleSystem and a trigger collider on this GameObject.
/// </summary>
public class GuidingSparkle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem sparkleParticles;

    [Header("Settings")]
    [SerializeField] private float destroyDelay = 3f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StopAndDestroy();
    }

    private void StopAndDestroy()
    {
        sparkleParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(gameObject, destroyDelay);
    }
}
