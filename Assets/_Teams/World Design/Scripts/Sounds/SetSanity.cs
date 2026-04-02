using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Code by Alonso
/// </summary>

public class SetSanity : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;

    [Header("Values")]
    private float originalValue = 0f;
    private float duration = 3f;
    private float currentValue;

    [Header("Reference")]
    private Coroutine routine;
    private EventInstance instance;
    private PARAMETER_ID sanityParameter;
    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.getDescription(out EventDescription desc);
        desc.getParameterDescriptionByName("SanityMeter", out PARAMETER_DESCRIPTION pdesc);
        sanityParameter = pdesc.id;

        currentValue = originalValue;

        instance.start();
        instance.setParameterByID(sanityParameter, currentValue);
    }

    //Function to set a new value to the sanity
    public void SetSanityF(float target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LerpSanity(target));
    }

    //Funtion to reset the sanity
    public void ResetSanity()
    {
        SetSanityF(originalValue);
    }

    //Coroutine that changes the sanity value in a progression
    private IEnumerator LerpSanity(float target)
    {
        float start = currentValue;
        float time = 0f;

        while (time < duration)
        {
            currentValue = Mathf.Lerp(start, target, time / duration);
            instance.setParameterByID(sanityParameter, currentValue);

            time += Time.deltaTime;
            yield return null;
        }

        currentValue = target;
        instance.setParameterByID(sanityParameter, currentValue);
    }

    private void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
