using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Code by Alonso
/// </summary>

public class SetStormMusic : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;

    [Header("Values")]
    private float originalValue = 0f;
    private float duration = 3f;
    private float currentValue;

    [Header("Reference")]
    private Coroutine routine;
    private EventInstance instance;
    private PARAMETER_ID stormParameter;
    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.getDescription(out EventDescription desc);
        desc.getParameterDescriptionByName("WeatherIntensity", out PARAMETER_DESCRIPTION pdesc);
        stormParameter = pdesc.id;

        currentValue = originalValue;

        instance.start();
        instance.setParameterByID(stormParameter, currentValue);
    }

    //Function to set a new value to the storm
    public void SetStormF(float target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LerpStorm(target));
    }

    //Funtion to reset the storm
    public void ResetStorm()
    {
        SetStormF(originalValue);
    }

    //Coroutine that changes the storm value in a progression
    private IEnumerator LerpStorm(float target)
    {
        float start = currentValue;
        float time = 0f;

        while (time < duration)
        {
            currentValue = Mathf.Lerp(start, target, time / duration);
            instance.setParameterByID(stormParameter, currentValue);

            time += Time.deltaTime;
            yield return null;
        }

        currentValue = target;
        instance.setParameterByID(stormParameter, currentValue);
    }

    private void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
