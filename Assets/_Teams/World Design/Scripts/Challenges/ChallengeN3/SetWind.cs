using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Code by Alonso
/// </summary>

public class SetWind : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;

    [Header("Values")]
    private float originalValue = 0f;
    private float duration = 3f;
    private float currentValue;

    [Header("Reference")]
    private Coroutine routine;
    private EventInstance instance;
    private PARAMETER_ID windParameter;
    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.getDescription(out EventDescription desc);
        desc.getParameterDescriptionByName("WeatherIntensity", out PARAMETER_DESCRIPTION pdesc);
        windParameter = pdesc.id;

        currentValue = originalValue;

        instance.start();
        instance.setParameterByID(windParameter, currentValue);
    }

    //Function to set a new value to the wind
    public void SetWindF(float target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LerpWind(target));
    }

    //Funtion to reset the wind
    public void ResetWind()
    {
        SetWindF(originalValue);
    }

    //Coroutine that changes the wind value in a progression
    private IEnumerator LerpWind(float target)
    {
        float start = currentValue;
        float time = 0f;

        while (time < duration)
        {
            currentValue = Mathf.Lerp(start, target, time / duration);
            instance.setParameterByID(windParameter, currentValue);

            time += Time.deltaTime;
            yield return null;
        }

        currentValue = target;
        instance.setParameterByID(windParameter, currentValue);
    }

    private void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
