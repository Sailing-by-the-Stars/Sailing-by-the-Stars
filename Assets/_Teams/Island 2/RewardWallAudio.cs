using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class RewardWallAudio : MonoBehaviour
{
    [SerializeField] private EventReference rewardWallEvent;
    [SerializeField] private float volume = 0.5f;

    private EventInstance rewardWallInstance;

    public void StartAudio()
    {
        if (!rewardWallInstance.isValid())
        {
            rewardWallInstance = RuntimeManager.CreateInstance(rewardWallEvent);
            rewardWallInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        rewardWallInstance.setVolume(volume);
        rewardWallInstance.start();
    }
}
