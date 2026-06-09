using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class StarTwinklingSound : MonoBehaviour
{
    [SerializeField] public EventReference fmodEvent;
    private EventInstance instance;

    bool hasStarted = false;

    public void PlaySound(ATTRIBUTES_3D Settings, float volume = 1)
    {
        if (!instance.isValid())
        {
            UnityEngine.Debug.LogError("sound not initialized correctly!!!");
        }

        instance.setVolume(volume);

        instance.set3DAttributes(Settings);
        if (instance.isValid() && !hasStarted)
        {
            instance.start();
            hasStarted = true;
        }
    }

    public void StopSound()
    {
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        hasStarted = false;
    }

    public void init()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);
    }

    private void OnDestroy()
    {
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }
}
