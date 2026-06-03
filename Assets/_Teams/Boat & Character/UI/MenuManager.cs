using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons (Top to Bottom order)")]
    public List<MenuButtonEntry> menuButtons = new();

    [Header("Shift Settings")]
    [SerializeField] private float shiftAmount = 15f;
    [SerializeField] private float shiftDuration = 0.2f;
    [SerializeField] private AnimationCurve shiftCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Animator Settings")]
    [SerializeField] private string animationStateName = "MenuHoverAnimation";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Blur Settings")]
    [SerializeField] private Image menuBackgroundImage;
    [SerializeField] private bool enableBlurEffect = true;

    [Header("Pause Menu")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private string gameSceneName = "MainGame";

    private bool _isPaused = false;
    private Dictionary<int, Coroutine> _fadeCoroutines = new Dictionary<int, Coroutine>();

    private int _currentHoveredIndex = -1;
    private Dictionary<int, Coroutine> _shiftCoroutines = new Dictionary<int, Coroutine>();
    private UIBlurEffect _blurEffect;

    private void Start()
    {
        if (enableBlurEffect && menuBackgroundImage != null)
        {
            _blurEffect = menuBackgroundImage.GetComponent<UIBlurEffect>();
            if (_blurEffect == null)
            {
                _blurEffect = menuBackgroundImage.gameObject.AddComponent<UIBlurEffect>();
            }
        }

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

    private void Update()
    {
        if ((TempStateMachine.Instance.gameState != GameState.Dialogue && TempStateMachine.Instance.gameState != GameState.Journal) && 
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)))
        {
            if (_isPaused) CloseMenu();
            else OpenMenu();
        }
    }

    public void OpenMenu()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        if (menuRoot != null) menuRoot.SetActive(true);
        TempStateMachine.Instance.SetState(_isPaused ? GameState.Dialogue : GameState.Moving);
    }

    public void CloseMenu()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (menuRoot != null) menuRoot.SetActive(false);
        TempStateMachine.Instance.SetState(_isPaused ? GameState.Dialogue : GameState.Moving);
    }

    public void OnReturnToGame()
    {
        CloseMenu();
    }

    public void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // This shouldn't be necessary, but Unity...
    public void OnDeveloperTools()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (menuRoot != null) menuRoot.SetActive(false);
    }

    public void OnLoadLastCheckpoint()
    {
        // Hook up your checkpoint system here
        Debug.Log("Load last checkpoint – not yet implemented");
        CloseMenu();
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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

        // Enable blur effect on first hover
        if (_blurEffect != null)
        {
            _blurEffect.EnableBlur();
        }

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

        // Disable blur effect when hovering exits
        if (_blurEffect != null)
        {
            _blurEffect.DisableBlur();
        }

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