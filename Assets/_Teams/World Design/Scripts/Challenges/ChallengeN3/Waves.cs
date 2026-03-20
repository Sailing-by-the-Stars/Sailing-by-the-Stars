using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Reflection;
using System;

/// <summary>
/// Code by Alonso
/// </summary>

public class Waves : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ocean;
    private Coroutine routine;

    [Header("Original value of the properties")]
    private float largeWindSpeedO = 30f;
    private float largeChaosO = 0.8f;
    private float firstBandO = 0;
    private float secondBandO = 0;
    private float windSpeedO = 4f;
    private float chaosO = 0.8f;
    private float windStart = 0f;

    [Header("Other values")]
    private float duration = 3f;

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
        WaterSurface ws = ocean.GetComponent<WaterSurface>();

        //We obtain the names of the wave parameters
        System.Type wsType = ws.GetType();
        FieldInfo fLargeWind = wsType.GetField("largeWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fLargeChaos = wsType.GetField("largeChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fFirstBand = wsType.GetField("largeBand0Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fSecondBand = wsType.GetField("largeBand1Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fWindSpeed = wsType.GetField("ripplesWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fChaos = wsType.GetField("ripplesChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float time = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (fLargeWind != null) fLargeWind.SetValue(ws, Mathf.Lerp(largeWindSpeedO, targetLargeWind, time));
            if (fLargeChaos != null) fLargeChaos.SetValue(ws, Mathf.Lerp(largeChaosO, targetLargeChaos, time));
            if (fFirstBand != null) fFirstBand.SetValue(ws, Mathf.Lerp(firstBandO, targetFirstBand, time));
            if (fSecondBand != null) fSecondBand.SetValue(ws, Mathf.Lerp(secondBandO, targetSecondBand, time));
            if (fWindSpeed != null) fWindSpeed.SetValue(ws, Mathf.Lerp(windSpeedO, targetWind, time));
            if (fChaos != null) fChaos.SetValue(ws, Mathf.Lerp(chaosO, targetChaos, time));

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

            if (fLargeWind != null) fLargeWind.SetValue(ws, Mathf.Lerp(startLargeWind, largeWindSpeedO, time));

            if (fLargeChaos != null) fLargeChaos.SetValue(ws, Mathf.Lerp(startLargeChaos, largeChaosO, time));

            if (fFirstBand != null) fFirstBand.SetValue(ws, Mathf.Lerp(startFirstBand, firstBandO, time));

            if (fSecondBand != null) fSecondBand.SetValue(ws, Mathf.Lerp(startSecondBand, secondBandO, time));

            if (fWindSpeed != null) fWindSpeed.SetValue(ws, Mathf.Lerp(startWind, windSpeedO, time));

            if (fChaos != null) fChaos.SetValue(ws, Mathf.Lerp(startChaos, chaosO, time));

            elapsed += Time.deltaTime;
            yield return null;
        }
    }



}
