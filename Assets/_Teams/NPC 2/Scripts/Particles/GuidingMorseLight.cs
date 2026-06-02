// Programmer: Arch

using System.Collections;
using UnityEngine;

/// <summary>
/// Flashes a guiding light in the Morse pattern for the letter L (. - . .).
/// Loops until StopMorsePattern() is called.
/// </summary>
public class GuidingMorseLight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light guidingLight;
    [SerializeField] private GameObject optionalGlowObject;

    [Header("Morse Timing (seconds)")]
    [SerializeField] private float dotDuration = 0.3f;
    [SerializeField] private float dashDuration = 0.9f;
    [SerializeField] private float symbolGap = 0.25f;
    [SerializeField] private float letterRepeatGap = 1.5f;

    private bool isRunning = false;

    private void Start()
    {
        SetLightOn(false);
        StartMorsePattern();
    }

    /// <summary>Starts the looping Morse flash pattern.</summary>
    public void StartMorsePattern()
    {
        if (isRunning) return;
        isRunning = true;
        StartCoroutine(LoopMorseL());
    }

    /// <summary>Stops the Morse flash pattern and turns the light off.</summary>
    public void StopMorsePattern()
    {
        isRunning = false;
        StopAllCoroutines();
        SetLightOn(false);
    }

    // Letter L in Morse: . - . .  (dot dash dot dot)
    private IEnumerator LoopMorseL()
    {
        while (isRunning)
        {
            yield return FlashSymbol(dotDuration);
            yield return new WaitForSeconds(symbolGap);

            yield return FlashSymbol(dashDuration);
            yield return new WaitForSeconds(symbolGap);

            yield return FlashSymbol(dotDuration);
            yield return new WaitForSeconds(symbolGap);

            yield return FlashSymbol(dotDuration);
            yield return new WaitForSeconds(letterRepeatGap);
        }
    }

    private IEnumerator FlashSymbol(float duration)
    {
        SetLightOn(true);
        yield return new WaitForSeconds(duration);
        SetLightOn(false);
    }

    private void SetLightOn(bool isOn)
    {
        if (guidingLight != null)
            guidingLight.enabled = isOn;

        if (optionalGlowObject != null)
            optionalGlowObject.SetActive(isOn);
    }
}
