using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MorseAudio : MonoBehaviour
{
    [SerializeField] private EventReference morseBeepEvent;
    [SerializeField] private float volume = 0.5f;

    private EventInstance beepInstance;

    public void StartBeep()
    {
        if (!beepInstance.isValid())
        {
            beepInstance = RuntimeManager.CreateInstance(morseBeepEvent);
        }

        beepInstance.setVolume(volume);
        beepInstance.start();
    }

    public void StopBeep()
    {
        if (!beepInstance.isValid()) return;
        
        beepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        beepInstance.release();
    }
}