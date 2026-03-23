/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;

/// <summary>
/// Triggers weather transitions on zone enter/exit. Suspends auto weather variation on enter and resumes on exit
/// (the zone has full control over weather while the player is inside).
/// </summary>
public class WeatherZoneEffect : MonoBehaviour, IZoneEffect
{
    [Header("Enter")]
    [Tooltip("Weather state to transition to when entering this zone")]
    [SerializeField] private WeatherState enterState;

    [Tooltip("Duration of the enter transition in seconds")]
    [SerializeField] private float enterDuration = 30f;

    [Tooltip("Curve shapes defining how weather builds into the enter state. " +
             "Leave null for a smooth default transition.")]
    [SerializeField] private WeatherTransitionCurves enterCurves;

    [Header("Exit")]
    [Tooltip("If ticked, exits to a random ambient state instead of exitState.")]
    [SerializeField] private bool randomAmbientOnExit = false;

    [Tooltip("Weather state to transition to when leaving this zone. " +
             "Explicitly set — this zone does not assume what the world looks like outside it.")]
    [SerializeField] private WeatherState exitState;

    [Tooltip("Duration of the exit transition in seconds.")]
    [SerializeField] private float exitDuration = 20f;

    [Tooltip("Curve shapes defining how weather fades from this zone's state. " +
             "Leave null for a smooth default transition.")]
    [SerializeField] private WeatherTransitionCurves exitCurves;

    public void OnEnter(GameObject instigator)
    {
        if (enterState == null)
        {
            Debug.Log("No enter state for weather event on " + gameObject.name);
            return;
        }
        Debug.Log("Weather transition called " + enterState);
        WeatherManager.Instance.SuspendAutoWeather();
        WeatherManager.Instance.TransitionTo(enterState, enterDuration, enterCurves);
    }
    public void OnExit(GameObject instigator)
    {
        // start timer for ambient weather
        WeatherManager.Instance.ResumeAutoWeather();

        WeatherState target;
        // chose random state or given exit state
        if (randomAmbientOnExit)
        {
            WeatherState random = WeatherManager.Instance.GetRandomAmbientState();
            target = random != null ? random : exitState;
        }
        else
        {
            target = exitState;
        }
        if (target == null)
        {
            Debug.Log("No exit state for weather event on " + gameObject.name);
            return;
        }
        WeatherManager.Instance.TransitionTo(target, exitDuration, exitCurves);
    }
}