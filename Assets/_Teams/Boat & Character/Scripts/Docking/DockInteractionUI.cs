// Created by Jantina
using System.Collections;
using TMPro;
using UnityEngine;
using Assets._Teams.Island_1.Scripts.User_Interface;

public class DockInteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject enterPrompt;
    [SerializeField] private GameObject exitPrompt;
    [SerializeField] private GameObject blockedPrompt;

    [Header("Prompt root CanvasGroup (wraps the three prompts above)")]
    [SerializeField] private CanvasGroup promptRoot;

    [Header("Black-screen overlay (separate full-screen Image)")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.35f;

    private enum DockState { Hidden, Board, Disembark, Blocked }
    private DockState _state = DockState.Hidden;
    private bool _suppressed = false;
    private DockPoint _currentDock;

    public bool IsShowingForDock(DockPoint dock) => _currentDock == dock;

    private void Start()
    {
        Debug.Log($"[DockUI] promptRoot={promptRoot} fadeCanvasGroup={fadeCanvasGroup} enterPrompt={enterPrompt} exitPrompt={exitPrompt} blockedPrompt={blockedPrompt}");
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        ApplyState();
    }


    public void Suppress(bool suppress)
    {
        _suppressed = suppress;
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.gameObject.SetActive(!suppress);
        ApplyState();
    }

    public void ShowBoard(bool canBoard, DockPoint source)
    {
        _currentDock = source;
        _state = DockState.Board;

        if (enterPrompt != null)
        {
            var tmp = enterPrompt.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.color = canBoard ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        }

        ApplyState();
    }

    public void ShowDisembark(DockPoint source)
    {
        _currentDock = source;
        _state = DockState.Disembark;
        ApplyState();
    }

    public void ShowBlockedReason(string reason, DockPoint source)
    {
        _currentDock = source;
        _state = DockState.Blocked;

        if (blockedPrompt != null)
        {
            var tmp = blockedPrompt.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = reason;
        }

        ApplyState();
    }

    public void Hide()
    {
        _currentDock = null;
        _state = DockState.Hidden;
        ApplyState();
    }

    private void ApplyState()
    {
        bool visible = _state != DockState.Hidden && !_suppressed;
        Debug.Log($"[DockUI] ApplyState state={_state} suppressed={_suppressed} visible={visible} promptRoot.alpha={promptRoot?.alpha}");
        enterPrompt?.SetActive(visible && _state == DockState.Board);
        exitPrompt?.SetActive(visible && _state == DockState.Disembark);
        blockedPrompt?.SetActive(visible && _state == DockState.Blocked);
        if (promptRoot != null)
        {
            promptRoot.alpha          = visible ? 1f : 0f;
            promptRoot.blocksRaycasts = visible;
            promptRoot.interactable   = visible;
        }
        if (TutorialManager.Instance != null && !_suppressed)
            TutorialManager.Instance.gameObject.SetActive(!visible);
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
        fadeCanvasGroup.alpha          = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}