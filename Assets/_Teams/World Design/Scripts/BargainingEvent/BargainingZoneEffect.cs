/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;

/// <summary>
/// Entry trigger for the Bargaining grief event.
/// Use with WorldEventZone to pair with controlled weather and music.
/// Does nothing if event is already completed or active.
/// OnExit is intentionally empty: event continues even if player leaves the trigger area.
/// </summary>
public class BargainingZoneEffect : MonoBehaviour, IZoneEffect
{
    private BargainingController controller;
    [SerializeField] private float audioIntensity = 1.0f;

    private void Start()
    {
        controller = FindFirstObjectByType<BargainingController>();
        if (controller == null)
        {
            Debug.LogWarning($"{gameObject.name}: No BargainingController found in scene.");
        }
    }
    public void OnEnter(GameObject instigator)
    {
        if (controller == null)
        {
            return;
        }
        if (controller.IsCompleted || controller.IsActive)
        {
            return;
        }
        controller.StartEvent(instigator);
    }
    public void OnExit(GameObject instigator) {}
}