using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Reflection;
using System;




/// Code by Alonso

public class ChallengeN3 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ocean;
    [SerializeField] private GameObject triggerZone;
    [SerializeField] private string targetTag = "Player";
    

    [Header("Original value of the properties")]
    private float largeWindSpeedO = 30f;
    private float largeChaosO = 0.8f;
    private float firstBandO = 0;
    private float secondBandO = 0;
    private float windSpeedO = 4f;
    private float chaosO = 0.8f;
    private float windStart = 0f;

    [Header ("Goal values of the properties")]
    private float targetLargeWind = 250f;
    private float targetLargeChaos = 1f;
    private float targetFirstBand = 1f;
    private float targetSecondBand = 1f;
    private float targetWind = 15f;
    private float targetChaos = 1f;
    private float windTarget = 4f;

    [Header("Bool")]
    [HideInInspector] public bool sounds = false;
    [HideInInspector] public event Action<bool> OnSoundsChanged;

    private void Start()
    {
        //windController = triggerZone.GetComponent<SetParameterByID>();
    }

    private void SetSounds(bool value)
    {
        if (sounds == value) return;
        sounds = value;
        OnSoundsChanged?.Invoke(sounds);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == targetTag)
        {
            StartChallenge();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == targetTag)
        {
            EndChallange();
        }
    }


    private void StartChallenge()
    {
        StartCoroutine(IncreaseValues(3f));
    }

    private void EndChallange()
    {
        StartCoroutine(DecreaseValues(3f));
    }

    //Coroutine to increase values
    private IEnumerator IncreaseValues(float duration)
    {
        WaterSurface ws = ocean.GetComponent<WaterSurface>();

        // Obtener campos por nombre (publicos o no)
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

            SetSounds(true);

            //if (windController != null) windController.wind = Mathf.Lerp(windStart, windTarget, time);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    //Coroutine to decrease values
    private IEnumerator DecreaseValues(float duration)
    {
        WaterSurface ws = ocean.GetComponent<WaterSurface>();

        System.Type wsType = ws.GetType();

        // Obtener campos
        FieldInfo fLargeWind = wsType.GetField("largeWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fLargeChaos = wsType.GetField("largeChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fFirstBand = wsType.GetField("largeBand0Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fSecondBand = wsType.GetField("largeBand1Multiplier", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fWindSpeed = wsType.GetField("ripplesWindSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        FieldInfo fChaos = wsType.GetField("ripplesChaos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // Valores iniciales (los actuales)
        float startLargeWind = fLargeWind != null ? (float)fLargeWind.GetValue(ws) : 0f;
        float startLargeChaos = fLargeChaos != null ? (float)fLargeChaos.GetValue(ws) : 0f;
        float startFirstBand = fFirstBand != null ? (float)fFirstBand.GetValue(ws) : 0f;
        float startSecondBand = fSecondBand != null ? (float)fSecondBand.GetValue(ws) : 0f;
        float startWind = fWindSpeed != null ? (float)fWindSpeed.GetValue(ws) : 0f;
        float startChaos = fChaos != null ? (float)fChaos.GetValue(ws) : 0f;
        //float startWindFMOD = windController != null ? windController.wind : 0f;

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

            //if (windController != null) windController.wind = Mathf.Lerp(startWindFMOD, windStart, time);

            SetSounds(false);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    //This funtion prints the water system properties
    private void PrintWaterProperties(WaterSurface ws)
    {
        SerializedObject so = new SerializedObject(ws);
        SerializedProperty prop = so.GetIterator();

        while (prop.NextVisible(true))
        {
            Debug.Log(prop.propertyPath);
        }
    }
}
