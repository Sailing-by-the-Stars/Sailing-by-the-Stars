using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons (Top to Bottom order)")]
    public List<MenuButtonEntry> menuButtons = new List<MenuButtonEntry>();

    [Header("Shift Settings")]
    [SerializeField] private float shiftAmount = 15f;
    [SerializeField] private float shiftDuration = 0.2f;
    [SerializeField] private AnimationCurve shiftCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Animator Settings")]
    [SerializeField] private string animationStateName = "MenuHoverAnimation";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.25f;

    private Dictionary<int, Coroutine> _fadeCoroutines = new Dictionary<int, Coroutine>();

    private int _currentHoveredIndex = -1;
    private Dictionary<int, Coroutine> _shiftCoroutines = new Dictionary<int, Coroutine>();

    private void Start()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            var entry = menuButtons[i];
            entry.rectTransform = entry.button.GetComponent<RectTransform>();
            entry.originalAnchoredPosition = entry.rectTransform.anchoredPosition;

            // Start background transparent
            if (entry.backgroundImage != null)
            {
                Color c = entry.backgroundImage.color;
                c.a = 0f;
                entry.backgroundImage.color = c;
            }

            if (entry.animator != null)
            {
                entry.animator.speed = 0f;
                entry.animator.Play(animationStateName, 0, 0f);
                entry.animator.Update(0f);
                entry.animator.speed = 1f;
                entry.animator.SetFloat("Speed", 0f);
            }

            int index = i;
            var hoverDetector = entry.button.gameObject.GetComponent<MenuButtonHoverDetector>();
            if (hoverDetector == null)
                hoverDetector = entry.button.gameObject.AddComponent<MenuButtonHoverDetector>();
            hoverDetector.Init(this, index);
        }
    }

    public void OnButtonHoverEnter(int index)
    {
        if (_currentHoveredIndex == index) return;

        if (_currentHoveredIndex >= 0)
            OnButtonHoverExit(_currentHoveredIndex);

        _currentHoveredIndex = index;
        var entry = menuButtons[index];

        if (entry.animator != null)
        {
            entry.animator.SetFloat("Speed", 1f);
            entry.animator.Play(animationStateName, 0, 0f);
        }

        // Fade background in via script
        StartFade(index, 1f);

        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (i < index)
                StartShift(i, menuButtons[i].originalAnchoredPosition + Vector2.up * shiftAmount);
            else if (i > index)
                StartShift(i, menuButtons[i].originalAnchoredPosition + Vector2.down * shiftAmount);
            else
                StartShift(i, menuButtons[i].originalAnchoredPosition);
        }
    }

    public void OnButtonHoverExit(int index)
    {
        if (_currentHoveredIndex != index) return;

        _currentHoveredIndex = -1;
        var entry = menuButtons[index];

        if (entry.animator != null)
        {
            entry.animator.SetFloat("Speed", -1f);
            entry.animator.Play(animationStateName, 0, 1f);
        }

        // Fade background out via script
        StartFade(index, 0f);

        for (int i = 0; i < menuButtons.Count; i++)
            StartShift(i, menuButtons[i].originalAnchoredPosition);
    }

    private void StartFade(int index, float targetAlpha)
    {
        if (_fadeCoroutines.TryGetValue(index, out var running) && running != null)
            StopCoroutine(running);

        _fadeCoroutines[index] = StartCoroutine(FadeBackground(index, targetAlpha));
    }

    private IEnumerator FadeBackground(int index, float targetAlpha)
    {
        Image img = menuButtons[index].backgroundImage;
        if (img == null) yield break;

        float startAlpha = img.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            Color c = img.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            img.color = c;
            yield return null;
        }

        Color final = img.color;
        final.a = targetAlpha;
        img.color = final;
    }

    private void StartShift(int index, Vector2 targetPos)
    {
        if (_shiftCoroutines.TryGetValue(index, out var running) && running != null)
            StopCoroutine(running);

        _shiftCoroutines[index] = StartCoroutine(ShiftButton(index, targetPos));
    }

    private IEnumerator ShiftButton(int index, Vector2 targetPos)
    {
        RectTransform rt = menuButtons[index].rectTransform;
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shiftDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = shiftCurve.Evaluate(Mathf.Clamp01(elapsed / shiftDuration));
            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, t);
            yield return null;
        }

        rt.anchoredPosition = targetPos;
    }
}

[System.Serializable]
public class MenuButtonEntry
{
    public Button button;
    public Image backgroundImage;
    public Animator animator;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Vector2 originalAnchoredPosition;
}