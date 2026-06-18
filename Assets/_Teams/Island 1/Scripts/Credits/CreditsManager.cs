using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The full-screen black panel that fades in over the game.")]
    [SerializeField] private CanvasGroup fadePanel;

    [Tooltip("The RectTransform of the credits text block that will scroll upward.")]
    [SerializeField] private RectTransform creditsTextRect;

    [Tooltip("Optional: a 'Thanks for playing!' label shown after scrolling ends.")]
    [SerializeField] private TextMeshProUGUI endLabel;

    [Header("Timing")]
    [Tooltip("How long the black fade-in takes before credits start.")]
    [SerializeField] private float fadeDuration = 2f;

    [Tooltip("How many units per second the credits scroll upward.")]
    [SerializeField] private float scrollSpeed = 80f;

    [Tooltip("How long the end label is shown before the game quits.")]
    [SerializeField] private float endHoldDuration = 5f;

    private void Awake()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(false);
        }

        if (creditsTextRect != null)
            creditsTextRect.gameObject.SetActive(false);

        if (endLabel != null)
            endLabel.gameObject.SetActive(false);
    }

    public void StartCredits()
    {
        StartCoroutine(CreditsSequence());
    }

    private IEnumerator CreditsSequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f; 
        
        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 1f;

        creditsTextRect.gameObject.SetActive(true);

        float screenHeight = Screen.height;
        Vector2 startPos = new Vector2(creditsTextRect.anchoredPosition.x, -screenHeight);
        creditsTextRect.anchoredPosition = startPos;

        float totalScrollDistance = (float)(2.5 * screenHeight) + creditsTextRect.rect.height;
        float scrolled = 0f;

        while (scrolled < totalScrollDistance)
        {
            float delta = scrollSpeed * Time.unscaledDeltaTime;
            creditsTextRect.anchoredPosition += Vector2.up * delta;
            scrolled += delta;
            yield return null;
        }

        creditsTextRect.gameObject.SetActive(false);

        if (endLabel != null)
        {
            endLabel.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(endHoldDuration);
            endLabel.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(endHoldDuration);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu6");
    }
}