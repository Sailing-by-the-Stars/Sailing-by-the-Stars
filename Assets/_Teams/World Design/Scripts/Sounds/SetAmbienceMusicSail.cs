using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SetAmbienceMusicSail : MonoBehaviour
{
    [SerializeField] private EventReference fmodEvent;
    [SerializeField] private float startDelay = 2f;

    private Coroutine routine;
    private EventInstance instance;
    private bool hasStarted = false;

    private void Awake()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);
    }

    public void PlayAmbienceMusic(float delay = -1f)
    {
        if (delay < 0f)
            delay = startDelay;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PlayAfterDelay(delay));
    }

    public void StopAmbienceMusic()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        hasStarted = false;
    }

    private IEnumerator PlayAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (instance.isValid() && !hasStarted)
        {
            instance.start();
            hasStarted = true;
        }

        routine = null;
    }

    private void OnDestroy()
    {
        if (routine != null)
            StopCoroutine(routine);

        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
}
