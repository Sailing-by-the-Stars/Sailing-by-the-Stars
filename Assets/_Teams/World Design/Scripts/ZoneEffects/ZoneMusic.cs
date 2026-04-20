/*
 * Created by Christina Pence
 * Contributed to by:
 */
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

/// <summary>
/// Generic zone music controller. Implements IZoneEffect so it works with WorldEventZone.
/// Fades music parameter in on zone enter and out on zone exit.
/// </summary>
public class ZoneEventMusic : MonoBehaviour, IZoneEffect
{
    [SerializeField] private EventReference fmodEvent;
    [SerializeField] private string parameterName = "EventMusicEQ";
    [SerializeField] private float targetIntensity = 1f;
    [SerializeField] private float lerpDuration = 3f;

    private float currentValue = 0f;
    private EventInstance instance;
    private PARAMETER_ID parameterId;
    private Coroutine routine;
    private float debugTimer = 0f;
    /*private void Update()
    {
        debugTimer += Time.deltaTime;
        if (debugTimer < 1f) return;
        debugTimer = 0f;

        instance.getTimelinePosition(out int posMs);
        int seconds = posMs / 1000;
        Debug.Log($"[ZoneEventMusic] {seconds / 60:00}:{seconds % 60:00}");
    }*/

    private void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.getDescription(out EventDescription desc);
        desc.getParameterDescriptionByName(parameterName, out PARAMETER_DESCRIPTION pdesc);
        parameterId = pdesc.id;
        instance.start();
        instance.setParameterByID(parameterId, currentValue);
    }

    public void OnEnter(GameObject instigator)
    {
        SetValue(targetIntensity);
    }

    public void OnExit(GameObject instigator)
    {
        SetValue(0f);
    }

    private void SetValue(float target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LerpValue(target));
    }

    private IEnumerator LerpValue(float target)
    {
        float start = currentValue;
        float time = 0f;
        while (time < lerpDuration)
        {
            currentValue = Mathf.Lerp(start, target, time / lerpDuration);
            instance.setParameterByID(parameterId, currentValue);
            time += Time.deltaTime;
            yield return null;
        }
        currentValue = target;
        instance.setParameterByID(parameterId, currentValue);
    }
    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
    }

    private void OnDestroy()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
    }
}