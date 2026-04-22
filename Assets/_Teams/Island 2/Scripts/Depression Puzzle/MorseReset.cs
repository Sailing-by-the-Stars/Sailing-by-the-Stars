using System.Collections;
using UnityEngine;

public class MorseReset : MonoBehaviour
{
    [SerializeField] private MorseDecoder morseDecoder;
    [SerializeField] private Light resetLight;

    [Header("Flash Settings")]
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float flashInterval = 0.15f;
    
    private Coroutine flashRoutine;

    private void OnEnable()
    {
        morseDecoder.OnMorseReset += Reset;
    }

    private void OnDisable()
    {
        morseDecoder.OnMorseReset -= Reset;
    }

    private void Reset()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashLight());
    }

    private IEnumerator FlashLight()
    {
        if (!resetLight) yield break;

        bool originalState = resetLight.enabled;

        for (int i = 0; i < flashCount; i++)
        {
            resetLight.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            resetLight.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }

        // Restore original state (in case it wasn't supposed to be on)
        resetLight.enabled = originalState;
    }
}