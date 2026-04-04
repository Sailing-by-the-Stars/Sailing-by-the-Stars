using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Code by Alonso
/// </summary>
public class SetBargainingEventMusic : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;

    [Header("Values")]
    private float originalValue = 0f;
    private float duration = 3f;
    private float currentValue;

    [Header("Reference")]
    private Coroutine routine;
    private EventInstance instance;
    private PARAMETER_ID eventMusicEQ;
    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.getDescription(out EventDescription desc);
        desc.getParameterDescriptionByName("EventMusicEQ", out PARAMETER_DESCRIPTION pdesc);
        eventMusicEQ = pdesc.id;

        currentValue = originalValue;

        instance.start();
        instance.setParameterByID(eventMusicEQ, currentValue);
    }

    //Function to set a new value to the bargaining music
    public void SetBargainingMusic(float target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LerpBargainingMusic(target));
    }

    //Funtion to reset the bargaining music
    public void ResetBargainingMusic()
    {
        SetBargainingMusic(originalValue);
    }

    //Coroutine that changes the bargaining music value in a progression
    private IEnumerator LerpBargainingMusic(float target)
    {
        float start = currentValue;
        float time = 0f;

        while (time < duration)
        {
            currentValue = Mathf.Lerp(start, target, time / duration);
            instance.setParameterByID(eventMusicEQ, currentValue);

            time += Time.deltaTime;
            yield return null;
        }

        currentValue = target;
        instance.setParameterByID(eventMusicEQ, currentValue);
    }

    private void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
