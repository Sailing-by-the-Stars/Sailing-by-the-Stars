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
    [SerializeField] private GameObject audioManager;
    [SerializeField] private GameObject waves;

    [Header ("Goal values of the properties")]
    private float targetLargeWind = 250f;
    private float targetLargeChaos = 1f;
    private float targetFirstBand = 1f;
    private float targetSecondBand = 1f;
    private float targetWind = 15f;
    private float targetChaos = 1f;
    private float windTarget = 4f;



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
        SetWind sw = audioManager.GetComponent<SetWind>();
        Waves w = waves.GetComponent<Waves>();

        //We call the function to change the wind and the waves
        w.ChangeWaves(targetLargeWind, targetLargeChaos, targetFirstBand, targetSecondBand, targetWind, targetChaos, windTarget);
        sw.SetWindF(0.8f); 

        yield return null;

    }

    //Coroutine to decrease values
    private IEnumerator DecreaseValues(float duration)
    {
        SetWind sw = audioManager.GetComponent<SetWind>();
        Waves w = waves.GetComponent<Waves>();

        w.ResetWaves(); //We call the function to reset the waves
        sw.ResetWind(); //We call the function to reset the wind
        yield return null;
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
