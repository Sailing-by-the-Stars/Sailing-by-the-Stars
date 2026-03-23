using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Code by Alonso
/// </summary>

public class SetThunderStrike : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;

    [Header("Values")]
    private float originalEQ = 0f;
    private float originalReverbDryr = 0f;
    private float originalVolume = 0f;

    private float currentEQ;
    private float currentReverbDryr;
    private float currentVolume;

    private float duration = 3f;

    [Header("Reference")]
    private Coroutine routine;
    private EventInstance instance;

    private PARAMETER_ID thunderStrikeEQParameter;
    private PARAMETER_ID thunderStrikeReverbDryrParameter;
    private PARAMETER_ID thunderStrikeVolumeParameter;
    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.getDescription(out EventDescription desc);

        desc.getParameterDescriptionByName("ThunderStrike_EQ", out PARAMETER_DESCRIPTION eqDesc);
        thunderStrikeEQParameter = eqDesc.id;

        desc.getParameterDescriptionByName("ThunderStrike_ReverbDryr", out PARAMETER_DESCRIPTION reverbDesc);
        thunderStrikeReverbDryrParameter = reverbDesc.id;

        desc.getParameterDescriptionByName("ThunderStrike_Volume", out PARAMETER_DESCRIPTION volumeDesc);
        thunderStrikeVolumeParameter = volumeDesc.id;

        currentEQ = originalEQ;
        currentReverbDryr = originalReverbDryr;
        currentVolume = originalVolume;

        instance.start();

        instance.setParameterByID(thunderStrikeEQParameter, currentEQ);
        instance.setParameterByID(thunderStrikeReverbDryrParameter, currentReverbDryr);
        instance.setParameterByID(thunderStrikeVolumeParameter, currentVolume);
    }

    // Function to set new values for thunder strike
    public void SetThunderStrikeF(float targetEQ, float targetReverbDryr, float targetVolume)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LerpThunderStrike(targetEQ, targetReverbDryr, targetVolume));
    }

    // Function to reset thunder strike
    public void ResetThunderStrike()
    {
        SetThunderStrikeF(originalEQ, originalReverbDryr, originalVolume);
    }

    // Coroutine that changes the three values in a progression
    private IEnumerator LerpThunderStrike(float targetEQ, float targetReverbDryr, float targetVolume)
    {
        float startEQ = currentEQ;
        float startReverbDryr = currentReverbDryr;
        float startVolume = currentVolume;

        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;

            currentEQ = Mathf.Lerp(startEQ, targetEQ, t);
            currentReverbDryr = Mathf.Lerp(startReverbDryr, targetReverbDryr, t);
            currentVolume = Mathf.Lerp(startVolume, targetVolume, t);

            instance.setParameterByID(thunderStrikeEQParameter, currentEQ);
            instance.setParameterByID(thunderStrikeReverbDryrParameter, currentReverbDryr);
            instance.setParameterByID(thunderStrikeVolumeParameter, currentVolume);

            time += Time.deltaTime;
            yield return null;
        }

        currentEQ = targetEQ;
        currentReverbDryr = targetReverbDryr;
        currentVolume = targetVolume;

        instance.setParameterByID(thunderStrikeEQParameter, currentEQ);
        instance.setParameterByID(thunderStrikeReverbDryrParameter, currentReverbDryr);
        instance.setParameterByID(thunderStrikeVolumeParameter, currentVolume);
    }

    private void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
