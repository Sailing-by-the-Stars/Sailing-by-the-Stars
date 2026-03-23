// Created by: Christina
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;
/// <summary>
/// Controls HDRP global fog. Single instance.
/// Subscribes to weather manager for weather events and handles transitions internally.
/// Use local volumes with blend distance for area-specific fog independent of weather.
/// Assign volume in inspector or tag with 'GlobalWeatherVolume'
/// </summary>
public class GlobalFogController : MonoBehaviour
{
    private static GlobalFogController instance;

    [Tooltip("Assign explicitly or use tag 'GlobalWeatherVolume' on intended volume")]
    [SerializeField] private Volume volume;
    [Tooltip("Default settings when no other weather state is given or active")]
    [SerializeField] private FogSettings defaultFogSettings;

    private Fog fogOverride;
    private Coroutine transitionRoutine;
    private FogSettings activeState;
    private const string volumeTag = "GlobalWeatherVolume";


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("Duplicate Fog Controllers in scene: destroying");
            Destroy(gameObject);
            return;
        }
        instance = this;
        // find global volume by tag if not assigned in inspector
        if (volume == null)
        {
            GameObject obj = GameObject.FindWithTag(volumeTag);
            if (obj == null)
            {
                Debug.LogWarning("No global volume assigned for fog effects");
                return;
            }
            volume = obj.GetComponent<Volume>();
        }
        if (!volume.profile.TryGet<Fog>(out fogOverride))
        {
            Debug.LogError(gameObject.name + ": no fog override found in volume profile");
            return;
        }
        // ensure override is enabled in volume profile
        fogOverride.enabled.Override(true);
        fogOverride.meanFreePath.Override(fogOverride.meanFreePath.value);
        fogOverride.baseHeight.Override(fogOverride.baseHeight.value);
        fogOverride.maximumHeight.Override(fogOverride.maximumHeight.value);
        fogOverride.albedo.Override(fogOverride.albedo.value);
        fogOverride.anisotropy.Override(fogOverride.anisotropy.value);
    }
    private void OnEnable()
    {
        if (WeatherManager.Instance  != null)
        {
            WeatherManager.Instance.OnWeatherTransitionStarted += StartWeatherBasedTransition;
            Debug.Log("Weather Manager Registered");
        }
    }
    private void OnDisable()
    {
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnWeatherTransitionStarted -= StartWeatherBasedTransition;
        }
    }
    // Event driven through weather manager
    // Handles transitions internally due to complex parameter types independent of controller interface (linear blend)
    private void StartWeatherBasedTransition(WeatherState newState, float duration)
    {
        if (fogOverride == null)
        {
            return;
        }
        // use default if no value is given
        FogSettings target = newState.fogSettings != null ? newState.fogSettings : defaultFogSettings;
        if (target == null || target == activeState)
        {
            return;
        }
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }
        activeState = target;
        transitionRoutine = StartCoroutine(TransitionTo(target, duration));
    }

    private IEnumerator TransitionTo(FogSettings target, float duration)
    {
        Debug.Log("Coroutine started");
        if (target == null || fogOverride == null)
        {
            Debug.Log("Coroutine canceled");
            yield break;
        }
        // store starting values (edge case for exiting and re-entering before fade to original settings was complete)
        float startMeanFreePath = fogOverride.meanFreePath.value;
        float startBaseHeight = fogOverride.baseHeight.value;
        float startMaximumHeight = fogOverride.maximumHeight.value;
        Color startAlbedo = fogOverride.albedo.value;
        float startAnisotropy = fogOverride.anisotropy.value;

        float t = 0f;
        duration = Mathf.Max(duration, 0.01f); // prevent division by 0
        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / duration);

            fogOverride.meanFreePath.value = Mathf.Lerp(startMeanFreePath, target.meanFreePath, ratio);
            fogOverride.baseHeight.value = Mathf.Lerp(startBaseHeight, target.baseHeight, ratio);
            fogOverride.maximumHeight.value = Mathf.Lerp(startMaximumHeight, target.maximumHeight, ratio);
            fogOverride.albedo.value = Color.Lerp(startAlbedo, target.albedo, ratio);
            fogOverride.anisotropy.value = Mathf.Lerp(startAnisotropy, target.anisotropy, ratio);

            yield return null;
        }
    }
}