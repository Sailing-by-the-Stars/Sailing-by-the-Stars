using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class StarFoundSound : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;
    private static EventInstance instance;

    public static void PlaySound()
    {
        if (!instance.isValid())
        {
            Debug.LogError("starfoundSound not initialized corectly!");
            return;
        }

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
