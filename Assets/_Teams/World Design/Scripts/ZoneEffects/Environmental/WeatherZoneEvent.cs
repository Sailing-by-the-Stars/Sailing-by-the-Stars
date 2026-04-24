/*
 * Created by Christina Pence
 * Contributed to by:
 */

using System.Collections.Generic;
using System.Collections;
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
             "Explicitly set � this zone does not assume what the world looks like outside it.")]
    [SerializeField] private WeatherState exitState;

    [Tooltip("Duration of the exit transition in seconds.")]
    [SerializeField] private float exitTransitionDuration = 20f;

    [Tooltip("Curve shapes defining how weather fades from this zone's state. " +
             "Leave null for a smooth default transition.")]
    [SerializeField] private WeatherTransitionCurves exitCurves;

    [Header("Linked weather objects")]
    [Tooltip("Used to link thunder events to the gamme zone")]
    [SerializeField] private List<GameObject> thunderSpawnerObjects;

    [Tooltip("Additional length of time to play exit state past autoweather settings in manager" +
        "Leave at 0 to use regular autoweather settings.")]
    [SerializeField] private float exitStateDuration = 0f;

    private static WeatherZoneEffect activeZone; // track current zone (in case of overlap)
    private Coroutine delayRoutine;

    public void OnEnter(GameObject instigator)
    {
        // prioritize most recently entered zone
        activeZone = this;

        if (delayRoutine != null)
        {
            StopCoroutine(delayRoutine);
            delayRoutine = null;
        }
        if (enterState == null || WeatherManager.Instance == null)
        {
            return;
        }
        WeatherManager.Instance.SuspendAutoWeather();
        WeatherManager.Instance.LinkThunderSpawnerObjects(thunderSpawnerObjects);
        WeatherManager.Instance.TransitionTo(enterState, enterDuration, enterCurves);
    }
    public void OnExit(GameObject instigator)
    {
        // ignore exit transition if another zone has taken over control
        if (activeZone != this)
        {
            return;
        }
        activeZone = null;

        WeatherState target = GetExitTarget();

        if (target == null)
        {
            WeatherManager.Instance.ResumeAutoWeather();
            return;
        }
        WeatherManager.Instance.TransitionTo(target, exitTransitionDuration, exitCurves);
        WeatherManager.Instance.ClearThunderSpawnerObjects();

        if (exitStateDuration != 0f)
        {
            delayRoutine = StartCoroutine(ResumeAfterDelay(exitStateDuration + exitTransitionDuration));
        }
        else
        {
            WeatherManager.Instance.ResumeAutoWeather();
        }
    }
    private IEnumerator ResumeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        WeatherManager.Instance.ResumeAutoWeather();
    }
    private WeatherState GetExitTarget()
    {
        if (randomAmbientOnExit)
        {
            WeatherState random = WeatherManager.Instance.GetRandomAmbientState();
            return random != null ? random : WeatherManager.Instance.DefaultState;
        }
        return exitState != null ? exitState : WeatherManager.Instance.DefaultState;
    }
}