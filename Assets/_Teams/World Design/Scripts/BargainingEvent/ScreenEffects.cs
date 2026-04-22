/*
 * Created by Christina Pence
 * Contributed to by:
 */
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Canvas))]

public class ScreenEffects : MonoBehaviour
{
    [Header("Screen Shake Defaults")]
    [SerializeField] private float defaultShakeDuration = 0.5f;
    [SerializeField] private float defaultShakeMagnitude = 0.3f;

    [Header("Flash Defaults")]
    [SerializeField] private float defaultFlashDuration = 1.2f;

    [Header("Vignette Defaults")]
    [SerializeField] private float defaultVignetteDuration = 2.0f;
    [SerializeField] private float defaultVignetteAlpha = 0.6f;

    private Image overlayImage;
    private Vector3? originalCameraPosition = null;
    private int activeShakes = 0;

    private void Awake()
    {
        GameObject overlayObj = new GameObject("ScreenEffectsOverlay");
        overlayObj.transform.SetParent(transform);
        overlayImage = overlayObj.AddComponent<Image>();
        overlayImage.rectTransform.anchorMin = Vector2.zero;
        overlayImage.rectTransform.anchorMax = Vector2.one;
        overlayImage.rectTransform.offsetMin = Vector2.zero;
        overlayImage.rectTransform.offsetMax = Vector2.zero;
        overlayImage.color = Color.clear;
        overlayImage.raycastTarget = false;
    }

    /// <summary>
    /// Shakes the main camera briefly.
    /// Uses default values if no overrides provided.
    /// </summary>
    public void ScreenShake(float magnitude = -1f, float duration = -1f)
    {
        float m = magnitude < 0f ? defaultShakeMagnitude : magnitude;
        float d = duration < 0f ? defaultShakeDuration : duration;
        StartCoroutine(ScreenShakeRoutine(m, d));
    }

    /// <summary>
    /// Flashes the screen with the given color.
    /// Uses default duration if not provided.
    /// </summary>
    public void Flash(Color color, float duration = -1f)
    {
        float d = duration < 0f ? defaultFlashDuration : duration;
        StartCoroutine(FlashRoutine(color, d));
    }

    /// <summary>
    /// Darkens the screen edges with a vignette of the given color.
    /// Uses default values if no overrides provided.
    /// </summary>
    public void Vignette(Color color, float alpha = -1f, float duration = -1f)
    {
        float a = alpha < 0f ? defaultVignetteAlpha : alpha;
        float d = duration < 0f ? defaultVignetteDuration : duration;
        Color c = new Color(color.r, color.g, color.b, a);
        StartCoroutine(FlashRoutine(c, d));
    }

    private IEnumerator ScreenShakeRoutine(float magnitude, float duration)
    {
        if (Camera.main == null)
        {
            yield break;
        }

        if (activeShakes == 0)
        {
            originalCameraPosition = Camera.main.transform.localPosition;
        }
        activeShakes++;

        Vector3 originalPos = originalCameraPosition.Value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            Camera.main.transform.localPosition = originalPos + Random.insideUnitSphere * strength;
            yield return null;
        }

        activeShakes--;
        if (activeShakes == 0)
        {
            Camera.main.transform.localPosition = originalCameraPosition.Value;
        }
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        if (overlayImage == null)
        {
            yield break;
        }

        float halfDuration = duration * 0.5f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            overlayImage.color = Color.Lerp(Color.clear, color, elapsed / halfDuration);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            overlayImage.color = Color.Lerp(color, Color.clear, elapsed / halfDuration);
            yield return null;
        }

        overlayImage.color = Color.clear;
    }
}