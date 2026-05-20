/*
 * Created by Christina Pence
 * Contributed to by:
 */
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

/// <summary>
/// Static ambient eye placed on the island for the Bargaining grief event.
/// Plays faint whispers when the tagged object enters the trigger collider.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class BargainingEye : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Shared audio data containing whisper voice lines.")]
    [SerializeField] private BargainingWhispers whispersAudio;
    [Tooltip("Volume of whispers played by this eye. Keep low for ambient effect.")]
    [SerializeField] private float whisperVolume = 0.3f;
    [Tooltip("Minimum pause between whisper lines in seconds.")]
    [SerializeField] private float whisperPauseMin = 3f;
    [Tooltip("Maximum pause between whisper lines in seconds.")]
    [SerializeField] private float whisperPauseMax = 8f;
    [Tooltip("Delay before first whisper plays after player enters trigger.")]
    [SerializeField] private float firstWhisperDelay = 0f;

    [Header("Activation")]
    [Tooltip("Tag of the object that activates this eye.")]
    [SerializeField] private string instigatorTag = "Player";

    private bool playerInRange = false;
    private Coroutine whisperRoutine;
    private EventInstance currentWhisperInstance;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(instigatorTag))
        {
            return;
        }
        playerInRange = true;
        whisperRoutine = StartCoroutine(WhisperRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(instigatorTag))
        {
            return;
        }
        playerInRange = false;
        StopWhispers();
    }

    private IEnumerator WhisperRoutine()
    {
        if (whispersAudio == null || whispersAudio.whisperLines == null || whispersAudio.whisperLines.Length == 0)
        {
            yield break;
        }
        if (firstWhisperDelay > 0f)
        {
            yield return new WaitForSeconds(firstWhisperDelay);
        }

        while (playerInRange)
        {
            ReleaseWhisperInstance();

            EventReference line = whispersAudio.whisperLines[Random.Range(0, whispersAudio.whisperLines.Length)];
            if (!line.IsNull)
            {
                currentWhisperInstance = RuntimeManager.CreateInstance(line);
                RuntimeManager.AttachInstanceToGameObject(currentWhisperInstance, gameObject);
                currentWhisperInstance.setVolume(whisperVolume);
                currentWhisperInstance.start();

                currentWhisperInstance.getDescription(out EventDescription description);
                description.getLength(out int lengthMs);
                yield return new WaitForSeconds(lengthMs / 1000f);
            }

            if (!playerInRange)
            {
                yield break;
            }

            yield return new WaitForSeconds(Random.Range(whisperPauseMin, whisperPauseMax));
        }
    }

    private void StopWhispers()
    {
        if (whisperRoutine != null)
        {
            StopCoroutine(whisperRoutine);
            whisperRoutine = null;
        }
        ReleaseWhisperInstance();
    }

    private void ReleaseWhisperInstance()
    {
        if (!currentWhisperInstance.isValid()) return;
        currentWhisperInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentWhisperInstance.release();
        currentWhisperInstance.clearHandle();
    }

    private void OnDestroy()
    {
        ReleaseWhisperInstance();
    }
}