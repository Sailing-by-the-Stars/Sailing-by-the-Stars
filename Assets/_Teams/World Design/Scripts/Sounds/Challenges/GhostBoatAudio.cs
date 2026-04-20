using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// 2D audio controller for the ghost boat challenge. Attach to the audio manager on the player.
/// Drives the clock parameter based on whether the player is looking at the ghost boat,
/// and plays a one-shot sound when the boat is spotted.
/// </summary>
public class GhostBoatAudio : MonoBehaviour
{
    [SerializeField] private string parameterName = "Clockon";
    [SerializeField] private EventReference ghostBoatEventAudio;
    [SerializeField] private EventReference boatStopSound;

    private EventInstance audioInstance;
    private EventInstance boatStopInstance;
    private PARAMETER_ID inViewParameterId;

    private void Start()
    {
        if (ghostBoatEventAudio.IsNull)
        {
            Debug.LogWarning($"{gameObject.name}: ghostBoatEventAudio is not assigned.");
            return;
        }
        audioInstance = RuntimeManager.CreateInstance(ghostBoatEventAudio);
        audioInstance.getDescription(out EventDescription desc);
        desc.getParameterDescriptionByName(parameterName, out PARAMETER_DESCRIPTION pdesc);
        inViewParameterId = pdesc.id;
        RuntimeManager.AttachInstanceToGameObject(audioInstance, gameObject);
        audioInstance.start();
        audioInstance.setParameterByID(inViewParameterId, 1f);
    }

    public void SetInView(bool inView)
    {
        if (audioInstance.isValid())
        {
            audioInstance.setParameterByID(inViewParameterId, inView ? 0f : 1f);
        }


        if (inView)
        {
            if (boatStopInstance.isValid())
            {
                boatStopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                boatStopInstance.release();
            }
            if (!boatStopSound.IsNull)
            {
                boatStopInstance = RuntimeManager.CreateInstance(boatStopSound);
                RuntimeManager.AttachInstanceToGameObject(boatStopInstance, gameObject);
                boatStopInstance.start();
            }
        }
    }
    public void ResetAudio()
    {
        if (audioInstance.isValid())
        {
            audioInstance.setParameterByID(inViewParameterId, 0f);
        }
        if (boatStopInstance.isValid())
        {
            boatStopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            boatStopInstance.release();
        }
    }

    private void OnDestroy()
    {
        if (audioInstance.isValid())
        {
            audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            audioInstance.release();
        }
        if (boatStopInstance.isValid())
        {
            boatStopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            boatStopInstance.release();
        }
    }

}