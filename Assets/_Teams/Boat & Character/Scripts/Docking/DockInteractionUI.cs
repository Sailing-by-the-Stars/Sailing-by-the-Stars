using System.Collections;
using TMPro;
using UnityEngine;

public class DockInteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject enterPrompt;
    [SerializeField] private GameObject exitPrompt;

    [Header("Fade Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.35f;

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowBoard(bool canBoard)
    {
        enterPrompt?.SetActive(true);
        exitPrompt?.SetActive(false);

        if (enterPrompt != null)
        {
            TextMeshProUGUI tmp = enterPrompt.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.color = canBoard ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        }
    }

    public void ShowDisembark()
    {
        enterPrompt?.SetActive(false);
        exitPrompt?.SetActive(true);
    }

    public void Hide()
    {
        enterPrompt?.SetActive(false);
        exitPrompt?.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;
        fadeCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}