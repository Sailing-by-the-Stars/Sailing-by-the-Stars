using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PagePickupSound : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;
    private EventInstance instance;

    public void PlaySound()
    {
        if (!instance.isValid())
            instance = RuntimeManager.CreateInstance(fmodEvent);

        instance.setVolume(3);
        instance.start();
    }
    
     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
