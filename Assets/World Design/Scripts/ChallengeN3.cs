using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;
using System.Runtime.CompilerServices;
using System.Collections;


/// Code by Alonso

public class ChallengeN3 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ocean;

    [Header("Original value of the properties")]
    private float largeWindSpeedO = 30f;
    private float largeChaosO = 0.8f;
    private float firstBandO = 0;
    private float secondBandO = 0;
    private float windSpeedO = 4f;
    private float chaosO = 0.8f;

    [Header ("Goal values of the properties")]
    private float targetLargeWind = 250f;
    private float targetLargeChaos = 1f;
    private float targetFirstBand = 1f;
    private float targetSecondBand = 1f;
    private float targetWind = 15f;
    private float targetChaos = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Boat")
        {
            StartChallenge();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Boat")
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

    private IEnumerator IncreaseValues(float duration)
    {
        WaterSurface ws = ocean.GetComponent<WaterSurface>();
        SerializedObject so = new SerializedObject(ws);

        //We find the properties of the water system
        SerializedProperty largeWindSpeed = so.FindProperty("largeWindSpeed"); //Wind speed in swell
        SerializedProperty largeChaos = so.FindProperty("largeChaos"); //Chaos in swell
        SerializedProperty firstband = so.FindProperty("largeBand0Multiplier"); //First band
        SerializedProperty secondBand = so.FindProperty("largeBand1Multiplier"); //Second band
        SerializedProperty windSpeed = so.FindProperty("ripplesWindSpeed"); //Local wind
        SerializedProperty chaos = so.FindProperty("ripplesChaos"); //Local chaos

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float time = elapsed / duration;
            time = Mathf.SmoothStep(0f, 1f, time);

            largeWindSpeed.floatValue = Mathf.Lerp(largeWindSpeedO, targetLargeWind, time);
            largeChaos.floatValue = Mathf.Lerp(largeChaosO, targetLargeChaos, time);
            firstband.floatValue = Mathf.Lerp(firstBandO, targetFirstBand, time);
            secondBand.floatValue = Mathf.Lerp(secondBandO, targetSecondBand, time);
            windSpeed.floatValue = Mathf.Lerp(windSpeedO, targetWind, time);
            chaos.floatValue = Mathf.Lerp(chaosO, targetChaos, time);

            so.ApplyModifiedProperties();

            elapsed += Time.deltaTime;
            yield return null;
        }
    }


    private IEnumerator DecreaseValues(float duration)
    {
        WaterSurface ws = ocean.GetComponent<WaterSurface>();
        SerializedObject so = new SerializedObject(ws);

        SerializedProperty largeWindSpeed = so.FindProperty("largeWindSpeed");
        SerializedProperty largeChaos = so.FindProperty("largeChaos");
        SerializedProperty firstband = so.FindProperty("largeBand0Multiplier");
        SerializedProperty secondBand = so.FindProperty("largeBand1Multiplier");
        SerializedProperty windSpeed = so.FindProperty("ripplesWindSpeed");
        SerializedProperty chaos = so.FindProperty("ripplesChaos");

        // leemos los valores actuales como punto de partida
        float startLargeWind = largeWindSpeed.floatValue;
        float startLargeChaos = largeChaos.floatValue;
        float startFirstBand = firstband.floatValue;
        float startSecondBand = secondBand.floatValue;
        float startWind = windSpeed.floatValue;
        float startChaos = chaos.floatValue;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float time = elapsed / duration;
            time = Mathf.SmoothStep(0f, 1f, time);

            largeWindSpeed.floatValue = Mathf.Lerp(startLargeWind, largeWindSpeedO, time);
            largeChaos.floatValue = Mathf.Lerp(startLargeChaos, largeChaosO, time);
            firstband.floatValue = Mathf.Lerp(startFirstBand, firstBandO, time);
            secondBand.floatValue = Mathf.Lerp(startSecondBand, secondBandO, time);
            windSpeed.floatValue = Mathf.Lerp(startWind, windSpeedO, time);
            chaos.floatValue = Mathf.Lerp(startChaos, chaosO, time);

            so.ApplyModifiedProperties();

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
