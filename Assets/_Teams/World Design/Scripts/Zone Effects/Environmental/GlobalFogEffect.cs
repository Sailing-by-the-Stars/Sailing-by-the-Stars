using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;

public class GlobalFogEffect : MonoBehaviour, IZoneEffect
{
    [SerializeField] private Volume volume;
    [SerializeField] private FogSettings targetFogSettings;
    [SerializeField] private float fadeDuration = 10f;

    private Fog fogOverride;
    private Coroutine fadeRoutine;

    // Original state
    private float originalMeanFreePath;
    private float originalBaseHeight;
    private float originalMaximumHeight;
    private Color originalAlbedo;
    private float originalAnisotropy;

    // track active effect to prevent accidental overlap in control
    private static GlobalFogEffect activeEffect;
    private void Awake()
    {
        if (volume == null)
        {
            volume = FindFirstObjectByType<Volume>();
            Debug.LogError(gameObject.name + ": no volume assigned");
            return;
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
        // store original values to revert on exit
        originalMeanFreePath = fogOverride.meanFreePath.value;
        originalBaseHeight = fogOverride.baseHeight.value;
        originalMaximumHeight = fogOverride.maximumHeight.value;
        originalAlbedo = fogOverride.albedo.value;
        originalAnisotropy = fogOverride.anisotropy.value;
    }

    public void OnEnter(GameObject instigator)
    {
        if (targetFogSettings == null)
        {
            Debug.LogWarning(gameObject.name + ": no target FogSettings assigned");
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeToSettings());
    }

    public void OnExit(GameObject instigator)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeToOriginal());
    }

    private IEnumerator FadeToSettings()
    {
        // store starting values (edge case for exiting and re-entering before fade to original settings was complete)
        float startMeanFreePath = fogOverride.meanFreePath.value;
        float startBaseHeight = fogOverride.baseHeight.value;
        float startMaximumHeight = fogOverride.maximumHeight.value;
        Color startAlbedo = fogOverride.albedo.value;
        float startAnisotropy = fogOverride.anisotropy.value;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / fadeDuration);

            fogOverride.meanFreePath.value = Mathf.Lerp(startMeanFreePath, targetFogSettings.meanFreePath, ratio);
            fogOverride.baseHeight.value = Mathf.Lerp(startBaseHeight, targetFogSettings.baseHeight, ratio);
            fogOverride.maximumHeight.value = Mathf.Lerp(startMaximumHeight, targetFogSettings.maximumHeight, ratio);
            fogOverride.albedo.value = Color.Lerp(startAlbedo, targetFogSettings.albedo, ratio);
            fogOverride.anisotropy.value = Mathf.Lerp(startAnisotropy, targetFogSettings.anisotropy, ratio);

            yield return null;
        }
    }

    private IEnumerator FadeToOriginal()
    {
        // store starting values (edge case for entering/exiting with incomplete coroutine)
        float startMeanFreePath = fogOverride.meanFreePath.value;
        float startBaseHeight = fogOverride.baseHeight.value;
        float startMaximumHeight = fogOverride.maximumHeight.value;
        Color startAlbedo = fogOverride.albedo.value;
        float startAnisotropy = fogOverride.anisotropy.value;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / fadeDuration);

            fogOverride.meanFreePath.value = Mathf.Lerp(startMeanFreePath, originalMeanFreePath, ratio);
            fogOverride.baseHeight.value = Mathf.Lerp(startBaseHeight, originalBaseHeight, ratio);
            fogOverride.maximumHeight.value = Mathf.Lerp(startMaximumHeight, originalMaximumHeight, ratio);
            fogOverride.albedo.value = Color.Lerp(startAlbedo, originalAlbedo, ratio);
            fogOverride.anisotropy.value = Mathf.Lerp(startAnisotropy, originalAnisotropy, ratio);

            yield return null;
        }
    }
}