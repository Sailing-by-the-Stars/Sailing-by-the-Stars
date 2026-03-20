using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Code by Alonso
/// </summary>
public class SetRainAndThunder : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;

    [Header("Values")]
    private float originalValueRain = 0f;
    private float currentValueRain;
    private float duration = 3f;
    private bool Thunder = false;

    [Header("Reference")]
    private Coroutine routine;
    private EventInstance instance;
    private PARAMETER_ID rainParameter;
    private PARAMETER_ID thunderParameter;

    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.getDescription(out EventDescription desc);

        desc.getParameterDescriptionByName("RainIntensity", out PARAMETER_DESCRIPTION rainDesc);
        rainParameter = rainDesc.id;

        desc.getParameterDescriptionByName("Thunder", out PARAMETER_DESCRIPTION thunderDesc);
        thunderParameter = thunderDesc.id;

        currentValueRain = originalValueRain;

        instance.start();
        instance.setParameterByID(rainParameter, currentValueRain);
        instance.setParameterByID(thunderParameter, Thunder ? 1f : 0f);
    }

    //Function to set a new value to the rain
    public void SetRain(float target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LerpRain(target));
    }

    //Funtion to reset the rain
    public void ResetRain()
    {
        SetRain(originalValueRain);
    }

    //Function to turn on the thunder
    public void SetThunder(bool on)
    {
        instance.setParameterByID(thunderParameter, on ? 1f : 0f);
    }

    //Function to turn off the thunder
    public void ResetThunder()
    {
        SetThunder(Thunder);
    }

    //Coroutine that changes the rain value in a progression
    private IEnumerator LerpRain(float target)
    {
        float start = currentValueRain;
        float time = 0f;

        while (time < duration)
        {
            currentValueRain = Mathf.Lerp(start, target, time / duration);
            instance.setParameterByID(rainParameter, currentValueRain);

            time += Time.deltaTime;
            yield return null;
        }

        currentValueRain = target;
        instance.setParameterByID(rainParameter, currentValueRain);
    }

    private void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
