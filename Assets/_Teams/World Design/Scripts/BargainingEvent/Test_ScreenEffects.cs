/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;

public class Test_ScreenEffects : MonoBehaviour
{
    private ScreenEffects effects;

    private void Start()
    {
        effects = FindFirstObjectByType<ScreenEffects>();
        if (effects == null)
        {
            Debug.LogWarning($"{gameObject.name}: no ScreenEffects found in scene.");
        }
    }

    [ContextMenu("Test Screen Shake (Default)")]
    private void TestScreenShakeDefault()
    {
        effects?.ScreenShake();
    }

    [ContextMenu("Test Screen Shake (Strong)")]
    private void TestScreenShakeStrong()
    {
        effects?.ScreenShake(magnitude: 0.8f, duration: 0.8f);
    }

    [ContextMenu("Test Screen Shake (Gentle)")]
    private void TestScreenShakeGentle()
    {
        effects?.ScreenShake(magnitude: 0.1f, duration: 0.3f);
    }

    [ContextMenu("Test Flash Red")]
    private void TestFlashRed()
    {
        effects?.Flash(new Color(1f, 0f, 0f, 0.6f));
    }

    [ContextMenu("Test Flash White")]
    private void TestFlashWhite()
    {
        effects?.Flash(new Color(1f, 1f, 1f, 0.5f));
    }

    [ContextMenu("Test Vignette Black")]
    private void TestVignetteBlack()
    {
        effects?.Vignette(Color.black);
    }

    [ContextMenu("Test Vignette Red (Warning)")]
    private void TestVignetteRedWarning()
    {
        effects?.Vignette(Color.red, alpha: 0.3f, duration: 1.5f);
    }

    [ContextMenu("Test Sequence1")]
    private void TestSequence1()
    {
        effects?.ScreenShake(magnitude: 0.3f, duration: 0.3f);
        effects?.Flash(new Color(1f, 0f, 0f, 0f));
        effects?.Vignette(Color.black);
    }

    [ContextMenu("Test Sequence2")]
    private void TestWarningSequence()
    {
        effects?.ScreenShake(magnitude: 0.1f, duration: 0.3f);
        effects?.Vignette(Color.red, alpha: 0.3f, duration: 1.2f);
    }
}