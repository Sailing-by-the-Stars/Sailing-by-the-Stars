// Created by Jantina
using System.Collections;
using UnityEngine;

public class BoatTutorialUI : MonoBehaviour
{
    public static BoatTutorialUI Instance;

    [Header("Panels — assign the root GameObject of each panel")]
    [SerializeField] private CanvasGroup interactPromptPanel;
    [SerializeField] private CanvasGroup leavePromptPanel;
    [SerializeField] private CanvasGroup haulAnchorPanel;
    [SerializeField] private CanvasGroup steerRudderPanel;
    [SerializeField] private CanvasGroup adjustSailPanel;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.2f;

    private CanvasGroup _activePanel;
    private Coroutine _fadeCoroutine;

    private void Awake() => Instance = this;
    private void Start()  => HideAll();

    public void ShowInteractPrompt() => TransitionTo(interactPromptPanel);
    public void ShowLeavePrompt()    => TransitionTo(leavePromptPanel);
    public void ShowHaulAnchor()     => TransitionTo(haulAnchorPanel);
    public void ShowSteerRudder()    => TransitionTo(steerRudderPanel);
    public void ShowAdjustSail()     => TransitionTo(adjustSailPanel);

    public void Hide()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        var toFade = _activePanel;
        _activePanel = null;
        if (toFade != null)
            _fadeCoroutine = StartCoroutine(FadePanel(toFade, 0f, () => SetVisible(toFade, false)));
    }

    private void TransitionTo(CanvasGroup next)
    {
        if (next == null || next == _activePanel) return;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossFade(_activePanel, next));
    }

    private IEnumerator CrossFade(CanvasGroup from, CanvasGroup to)
    {
        // Bring 'to' into the hierarchy visible but transparent
        SetVisible(to, true);
        to.alpha = 0f;

        float elapsed = 0f;
        float fromAlpha = from != null ? from.alpha : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            if (from != null) from.alpha = Mathf.Lerp(fromAlpha, 0f, t);
            to.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        if (from != null) { from.alpha = 0f; SetVisible(from, false); }
        to.alpha     = 1f;
        _activePanel = to;
    }

    private IEnumerator FadePanel(CanvasGroup cg, float target, System.Action onDone = null)
    {
        float elapsed = 0f;
        float start   = cg.alpha;
        while (elapsed < fadeDuration)
        {
            elapsed  += Time.deltaTime;
            cg.alpha  = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        cg.alpha = target;
        onDone?.Invoke();
    }

    private void HideAll()
    {
        SetVisible(interactPromptPanel, false);
        SetVisible(leavePromptPanel,    false);
        SetVisible(haulAnchorPanel,     false);
        SetVisible(steerRudderPanel,    false);
        SetVisible(adjustSailPanel,     false);
        _activePanel = null;
    }

    private static void SetVisible(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha          = visible ? 1f : 0f;
        cg.interactable   = visible;
        cg.blocksRaycasts = visible;
        cg.gameObject.SetActive(visible);
    }
}