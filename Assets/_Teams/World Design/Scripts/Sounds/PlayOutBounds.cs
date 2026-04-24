using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Code by: Alonso
/// </summary>

public class PlayOutBounds : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;

    private EventInstance instance;

    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);
    }

    public void PlaySound()
    {
        if (!instance.isValid())
            instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.start();
    }

    public void StopSound()
    {
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        // Si lo quieres cortar de golpe:
        // instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
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
