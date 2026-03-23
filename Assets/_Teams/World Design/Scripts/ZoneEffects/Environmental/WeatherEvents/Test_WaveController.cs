using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents;

// Code by Alonso
// Modified by Christina
/// <summary>
/// Controls HDRP WaterSurface wave parameters via reflection.
/// Receives wave intensity from 0-1 from weather management system and maps to given parameter ranges.
/// Min values are the baseline values at calm (0). Max values are highest values in strongest storm.
/// These values can be adjusted to give the appropriate range but should be within the water surface parameters.
/// </summary>
public class Test_WaveController: MonoBehaviour, IWeatherEventController
{
    [Header("References")]
    [SerializeField] private WaterSurface ocean;
    private Coroutine routine;

    [Header("Large wave parameters")]
    [Tooltip("Higher values cause bigger waves (distant wind speed)")]
    [Range(0f, 250f)][SerializeField] private float minLargeWindSpeed = 5f;
    [Range(0f, 250f)][SerializeField] private float maxLargeWindSpeed = 60f;

    [Tooltip("Higher values give choppier appearance")]
    [Range(0f, 1f)][SerializeField] private float minLargeChaos = 0.2f;
    [Range(0f, 1f)][SerializeField] private float maxLargeChaos = 1f;

    [Tooltip("Suppresses amplitude of the wave bands (lower values = smaller waves)")]
    [Range(0f, 1f)][SerializeField] private float minFirstBandMultiplier = 0.2f;
    [Range(0f, 1f)][SerializeField] private float maxFirstBandMultiplier = 1f;
    [Range(0f, 1f)][SerializeField] private float minSecondBandMultiplier = 0.2f;
    [Range(0f, 1f)][SerializeField] private float maxSecondBandMultiplier = 1f;

    [Header("Ripple parameters")]
    [Tooltip("Influences shape and maximum amplitude (local wind)")]
    [Range(0f, 15f)][SerializeField] private float minRipplesWindSpeed = 4f;
    [Range(0f, 15f)][SerializeField] private float maxRipplesWindSpeed = 10f;

    [Range(0f, 1f)][SerializeField] private float minRipplesChaos = 0.2f;
    [Range(0f, 1f)][SerializeField] private float maxRipplesChaos = 1f;

    // Cached references
    private FieldInfo fLargeWindSpeed;
    private FieldInfo fLargeChaos;
    private FieldInfo fFirstBand;
    private FieldInfo fSecondBand;
    private FieldInfo fRipplesWindSpeed;
    private FieldInfo fRipplesChaos;

    private float lastWaveIntensity = -1f;
    private const float waveChangeThreshold = 0.005f;

    [Header("Other values")]
    private float duration = 3f;

    private void Awake()
    {
        if (ocean == null)
        {
            ocean = FindFirstObjectByType<WaterSurface>();
        }
        if (ocean == null)
        {
            Debug.LogWarning("No water surface assigned in wave controller.");
        }
        //We obtain the names of the wave parameters
        // References cached to prevent multiple lookups
        System.Type wsType = ocean.GetType();
        fLargeWindSpeed = wsType.GetField("largeWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fLargeChaos = wsType.GetField("largeChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fFirstBand = wsType.GetField("largeBand0Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fSecondBand = wsType.GetField("largeBand1Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fRipplesWindSpeed = wsType.GetField("ripplesWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fRipplesChaos = wsType.GetField("ripplesChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        LogMissingFields();

        WeatherManager.Instance.Register(this);
    }
    /// <summary>
    /// Receives wave intensity value pre-blended by weather system and maps onto defined min and max range per value.
    /// Does not blend values internally.
    /// </summary>
    /// <param name="values"></param>
    public void ChangeWeatherEventValues(WeatherValues values)
    {
        if (ocean == null)
        {
            Debug.Log("No watersurface found");
            return;
        }

        float t = Mathf.Clamp01(values.waveIntensity);
        // only transition significant changes
        if (Mathf.Abs(t - lastWaveIntensity) < waveChangeThreshold)
        {
            return;
        }
        lastWaveIntensity = t;
        // get value to match normalized intensity  
        fLargeWindSpeed?.SetValue(ocean, Mathf.Lerp(minLargeWindSpeed, maxLargeWindSpeed, t));
        fLargeChaos?.SetValue(ocean, Mathf.Lerp(minLargeChaos, maxLargeChaos, t));
        fFirstBand?.SetValue(ocean, Mathf.Lerp(minFirstBandMultiplier, maxFirstBandMultiplier, t));
        fSecondBand?.SetValue(ocean, Mathf.Lerp(minSecondBandMultiplier, maxSecondBandMultiplier, t));
        fRipplesWindSpeed?.SetValue(ocean, Mathf.Lerp(minRipplesWindSpeed, maxRipplesWindSpeed, t));
        fRipplesChaos?.SetValue(ocean, Mathf.Lerp(minRipplesChaos, maxRipplesChaos, t));
    }

    //Function to change the values of the waves
    public void ChangeWaves(float targetLargeWind, float targetLargeChaos, float targetFirstBand, float targetSecondBand, float targetWind, float targetChaos, float windTarget)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(WaveChangeLerp(targetLargeWind, targetLargeChaos, targetFirstBand, targetSecondBand, targetWind, targetChaos, windTarget));
    }

    //Funtion to reset the values of the waves
    public void ResetWaves()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ResetWaveLerp());
    }

    //Coroutine that changes the wind value in a progression
    private IEnumerator WaveChangeLerp(float targetLargeWind, float targetLargeChaos, float targetFirstBand, float targetSecondBand, float targetWind, float targetChaos, float windTarget)
    {



        float elapsed = 0f;

        while (elapsed < duration)
        {
            float time = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (fLargeWindSpeed != null) fLargeWindSpeed.SetValue(ocean, Mathf.Lerp(minLargeWindSpeed, targetLargeWind, time));
            if (fLargeChaos != null) fLargeChaos.SetValue(ocean, Mathf.Lerp(minLargeChaos, targetLargeChaos, time));
            if (fFirstBand != null) fFirstBand.SetValue(ocean, Mathf.Lerp(minFirstBandMultiplier, targetFirstBand, time));
            if (fSecondBand != null) fSecondBand.SetValue(ocean, Mathf.Lerp(minSecondBandMultiplier, targetSecondBand, time));
            if (fRipplesWindSpeed != null) fRipplesWindSpeed.SetValue(ocean, Mathf.Lerp(minRipplesWindSpeed, targetWind, time));
            if (fRipplesChaos != null) fRipplesChaos.SetValue(ocean, Mathf.Lerp(minRipplesChaos, targetChaos, time));

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ResetWaveLerp()
    {
        WaterSurface ws = ocean.GetComponent<WaterSurface>();
        System.Type wsType = ws.GetType();

        //We obtain the names of the wave parameters
        FieldInfo fLargeWind = wsType.GetField("largeWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fLargeChaos = wsType.GetField("largeChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fFirstBand = wsType.GetField("largeBand0Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fSecondBand = wsType.GetField("largeBand1Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fWindSpeed = wsType.GetField("ripplesWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fChaos = wsType.GetField("ripplesChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        //Initial values
        float startLargeWind = fLargeWind != null ? (float)fLargeWind.GetValue(ws) : 0f;
        float startLargeChaos = fLargeChaos != null ? (float)fLargeChaos.GetValue(ws) : 0f;
        float startFirstBand = fFirstBand != null ? (float)fFirstBand.GetValue(ws) : 0f;
        float startSecondBand = fSecondBand != null ? (float)fSecondBand.GetValue(ws) : 0f;
        float startWind = fWindSpeed != null ? (float)fWindSpeed.GetValue(ws) : 0f;
        float startChaos = fChaos != null ? (float)fChaos.GetValue(ws) : 0f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float time = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (fLargeWind != null) fLargeWind.SetValue(ws, Mathf.Lerp(startLargeWind, minLargeWindSpeed, time));

            if (fLargeChaos != null) fLargeChaos.SetValue(ws, Mathf.Lerp(startLargeChaos, minLargeChaos, time));

            if (fFirstBand != null) fFirstBand.SetValue(ws, Mathf.Lerp(startFirstBand, minFirstBandMultiplier, time));

            if (fSecondBand != null) fSecondBand.SetValue(ws, Mathf.Lerp(startSecondBand, minSecondBandMultiplier, time));

            if (fWindSpeed != null) fWindSpeed.SetValue(ws, Mathf.Lerp(startWind, minRipplesWindSpeed, time));

            if (fChaos != null) fChaos.SetValue(ws, Mathf.Lerp(startChaos, minRipplesChaos, time));

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void LogMissingFields()
    {
        if (fLargeWindSpeed == null) Debug.LogWarning("WaveController: Field 'largeWindSpeed' not found on WaterSurface.");
        if (fLargeChaos == null) Debug.LogWarning("WaveController: Field 'largeChaos' not found on WaterSurface.");
        if (fFirstBand == null) Debug.LogWarning("WaveController: Field 'largeBand0Multiplier' not found on WaterSurface.");
        if (fSecondBand == null) Debug.LogWarning("WaveController: Field 'largeBand1Multiplier' not found on WaterSurface.");
        if (fRipplesWindSpeed == null) Debug.LogWarning("WaveController: Field 'ripplesWindSpeed' not found on WaterSurface.");
        if (fRipplesChaos == null) Debug.LogWarning("WaveController: Field 'ripplesChaos' not found on WaterSurface.");
    }
    public void ChangeDirection(Vector3 direction) { }       // required by interface; currently not applicable for waves
    public void SetRandomEventsActive(bool isActive) { }    // required by interface; currently not applicable for waves
    //Function to change the values of the waves
}

