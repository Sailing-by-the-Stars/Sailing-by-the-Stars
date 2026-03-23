/*
 * Created by Christina Pence
 * Contributed to by:
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents;

// used for random variation of weather states - values do not need to add up to a specific amount
[Serializable]
public struct AmbientWeatherState
{
    public WeatherState state;
    [Tooltip("Relative chance of this state being selected (higher is more likely)")]
    public float weight;
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("Default")]
    [SerializeField] private WeatherState defaultState; // applied immediately on scene start
    
    [Tooltip("States to randomly cycle between during normal sailing " +
        "(leave empty to disable auto variation, include default state if using)")]
    [SerializeField] private AmbientWeatherState[] ambientStates;
    [Tooltip("Length of time before choosing a new state. Time is not counted during transitions.")]
    [SerializeField] private float minAmbientChangeInterval = 60f;
    [SerializeField] private float maxAmbientChangeInterval = 300f;
    [SerializeField] private float ambientChangeTransitionTime = 20f;

    // Controllers
    private readonly List<IWeatherEventController> controllers = new List<IWeatherEventController>();
    private WindController windController;

    // State and transitions
    private WeatherState activeState;
    private Coroutine activeTransition;
    private Coroutine ambientCycle;
    private bool autoWeatherSuspended;
    
    // Used for blending
    private WeatherValues snapshotValues; // used to capture values directly before transition
    private WeatherValues currentValues;

    public Vector3 WindVelocity => windController != null ? windController.currentWindVelocity 
        : ConvertToVector(currentValues.windSpeed, currentValues.windDirectionDegrees); // fallback

    /// <summary>
    /// Register a controller to receive weather updates. Call in Awake.
    /// </summary>
    public void Register(IWeatherEventController controller)
    {
        controllers.Add(controller);
        if (controller is WindController wind)
        {
            windController = wind;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (defaultState == null)
        {
            Debug.LogWarning("No default state assigned in Weather Manager");
            return;
        }
        currentValues = defaultState.values;
        activeState = defaultState;

        if (ambientStates != null && ambientStates.Length > 0)
        {
            ambientCycle = StartCoroutine(CycleAmbientWeather());
        }
        
        PushToControllers();     
    }
    
    private IEnumerator CycleAmbientWeather()
    {
        while (true)
        {
            if (autoWeatherSuspended || activeTransition != null)
            {
                yield return null;
            }
            else
            {
                float interval = UnityEngine.Random.Range(minAmbientChangeInterval, maxAmbientChangeInterval);
                yield return new WaitForSeconds(interval);

                WeatherState next = GetRandomAmbientState();
                // ignore irrelevant changes due to probability settings
                if (next == null || next == activeState)
                {
                    continue;
                }
                TransitionTo(next, ambientChangeTransitionTime);
            }
        }
    }
    /// <summary>
    /// Randomly selects one of the ambient weather states based on weighted probability.
    /// </summary>
    public WeatherState GetRandomAmbientState()
    {
        if (ambientStates == null || ambientStates.Length == 0)
        {
            return null;
        }
        float total = 0f;
        foreach (AmbientWeatherState ambientState in ambientStates)
        {
            total += ambientState.weight;
        }

        float randomValue = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;
        foreach (AmbientWeatherState ambientState in ambientStates)
        {
            cumulative += ambientState.weight;
            if (randomValue <= cumulative)
            {
                return ambientState.state;
            }
        }
        return null;

    }
    /// <summary>
    /// Suspend auto weather variation (for example for scripted events or inside trigger areas)
    /// </summary>
    public void SuspendAutoWeather()
    {
        autoWeatherSuspended = true;
        if (ambientCycle != null)
        {
            StopCoroutine(ambientCycle);
            ambientCycle = null;
        }
    }
    /// <summary>
    /// Resume auto weather variation (for example when exiting scripted events or triggers)
    /// </summary>
    public void ResumeAutoWeather()
    {
        autoWeatherSuspended = false;
        if (ambientStates != null && ambientStates.Length > 0)
        {
            ambientCycle = StartCoroutine(CycleAmbientWeather());
        }

    }
    /// <summary>
    /// Transitions to target state over the given duration.
    /// If given, uses the animation curves to control blend between current and target state
    /// If no curves are given, uses a default ease for all parameters.
    /// </summary>
    public void TransitionTo(WeatherState target, float duration, WeatherTransitionCurves curves = null)
    {
        // TODO: May need to turn off auto roll here too during transition
        if (target == null)
        {
            Debug.LogError("Weather manager TransitionTo called with null state.");
            return;
        }
        snapshotValues = currentValues;
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }
        activeState = target;
        activeTransition = StartCoroutine(RunTransition(target, duration, curves));

        // values that only need to be applied once without blending during transition
        currentValues.windRandomEventsActive = target.values.windRandomEventsActive;
        currentValues.windAutoRerollIntensity = target.values.windAutoRerollIntensity;
    }

    private IEnumerator RunTransition(WeatherState target, float duration, WeatherTransitionCurves curves)
    {
        float elapsed = 0f;
        duration = Mathf.Max(duration, 0.01f); // prevent division by zero

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ApplyAtTime(target, Mathf.Clamp01(elapsed / duration), curves);
            yield return null;
        }

        ApplyAtTime(target, 1f, curves); // apply final values (1 represents fully elapsed duration)
        activeTransition = null;
    }

    private void ApplyAtTime(WeatherState target, float t, WeatherTransitionCurves curves)
    {
        // Wind 
        currentValues.windSpeed = BlendValue(snapshotValues.windSpeed, target.values.windSpeed,
                                             t, curves, c => c.windCurve);

        // normalize degrees to 0-360 to prevent negative values from LerpAngle's shortest path
        currentValues.windDirectionDegrees = (Mathf.LerpAngle(snapshotValues.windDirectionDegrees,
                                                              target.values.windDirectionDegrees, t) + 360f) % 360f;
        
        /* TODO: the current reroll multiplies a random direcion vector by the intensity
         * effectively changing magnitude rather than limiting variance around the base direction
         * AutoRerollIntensity does not need to blend during transition as it is on a timer
       
        currentValues.windAutoRerollIntensity = BlendValue(
            snapshotValues.windAutoRerollIntensity,
            target.values.windAutoRerollIntensity,
            t, curves, c => c.windCurve
        );
        */

        // Rain 
        currentValues.rainIntensity = BlendValue(snapshotValues.rainIntensity, target.values.rainIntensity,
                                                 t, curves, c => c.rainCurve);

        // Ocean
        currentValues.waveIntensity = BlendValue(snapshotValues.waveIntensity, target.values.waveIntensity,
                                                 t, curves, c => c.waveCurve);

        currentValues.oceanCurrentSpeed = BlendValue(snapshotValues.oceanCurrentSpeed, target.values.oceanCurrentSpeed,
                                                     t, curves, c => c.currentCurve);

        // Thunder
        currentValues.thunderIntensity = BlendValue(snapshotValues.thunderIntensity, target.values.thunderIntensity,
                                                    t, curves, c => c.thunderCurve);

        PushToControllers();
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void PushToControllers()
    {
        foreach (IWeatherEventController controller in controllers)
        {
            controller.ChangeWeatherEventValues(currentValues);
        }
        
    }
    /// <summary>
    /// Helper that blends a single weather parameter from snapshot to target value.
    /// Use for parameters that blend using a curve in WeatherValues only.
    /// </summary>
    private static float BlendValue(float snapshotValue, float targetValue, float t,
                                    WeatherTransitionCurves allWeatherCurves,
                                    Func<WeatherTransitionCurves, AnimationCurve> getCurveForParameter) // caller selects target curve for parameter
    {
        AnimationCurve curve = allWeatherCurves != null ? getCurveForParameter(allWeatherCurves) : null;
        // use default transition if no curve is found for the parameter
        float blend = curve != null ? Mathf.Clamp01(curve.Evaluate(t)) : Mathf.SmoothStep(0f, 1f, t); 

        // blend is 0-1 weight: 0 = fully at snapshot, 1 = fully at target
        return Mathf.Lerp(snapshotValue, targetValue, blend);
    }

    private static Vector3 ConvertToVector(float magnitude, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * magnitude;
    }
}