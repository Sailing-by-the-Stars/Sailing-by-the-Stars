using FMODUnity;
using FMOD.Studio;
using UnityEngine;

/// <summary>
/// Drives a 0-1 FMOD parameter for Bargaining event sound representing closeness of entity/ time remaining.
public class SetBargainingTimer : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;
    private const string parameterName = "EventMusicEQ";

    private EventInstance instance;
    private PARAMETER_ID parameterId;

    private void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);
        instance.getDescription(out EventDescription desc);
        desc.getParameterDescriptionByName(parameterName, out PARAMETER_DESCRIPTION pdesc);
        parameterId = pdesc.id;
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        RuntimeManager.AttachInstanceToGameObject(instance, gameObject);
        instance.start();
    }

    public void SetIntensity(float value)
    {
        Debug.Log("audio intensity " +  value);
        instance.setParameterByID(parameterId, value);
    }

    public void ResetIntensity()
    {
        instance.setParameterByID(parameterId, 0f);
    }

    private void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}