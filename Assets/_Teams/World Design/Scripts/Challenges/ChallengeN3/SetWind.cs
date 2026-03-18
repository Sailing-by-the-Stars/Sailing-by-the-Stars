using System;
using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Code by Alonso
/// </summary>

public class SetWind : MonoBehaviour
{
    private EventInstance instance;
    private PARAMETER_ID windParameter;

    [SerializeField] private EventReference fmodEvent;
    [SerializeField] private ChallengeN3 challenge;

    [Header("Settings")]
    private float valueWhenOn = 0.8f;
    private float valueWhenOff = 0f;
    private float changeDuration = 3f;
    private float currentValue = 0f;

    [Header("Reference")]
    private Coroutine changeCoroutine;
    
    /*[Range(-12f, 12f)]
    [HideInInspector] public float wind;*/
    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        EventDescription desc;
        instance.getDescription(out desc);
        PARAMETER_DESCRIPTION pdesc;
        desc.getParameterDescriptionByName("WeatherIntensity", out pdesc);
        windParameter = pdesc.id;

        // Inicia parámetro
        instance.start();
        instance.setParameterByID(windParameter, currentValue);

        if (challenge == null)
            challenge = FindObjectOfType<ChallengeN3>();

        if (challenge != null)
            challenge.OnSoundsChanged += HandleSoundsChanged;
    }

    private void HandleSoundsChanged(bool on)
    {
        float target = on ? valueWhenOn : valueWhenOff;
        if (changeCoroutine != null) StopCoroutine(changeCoroutine);
        changeCoroutine = StartCoroutine(ChangeParamRoutine(currentValue, target, changeDuration));
    }

    private IEnumerator ChangeParamRoutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            currentValue = Mathf.Lerp(from, to, t);
            instance.setParameterByID(windParameter, currentValue);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentValue = to;
        instance.setParameterByID(windParameter, currentValue);
        changeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (challenge != null)
            challenge.OnSoundsChanged -= HandleSoundsChanged;
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
