using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the main menu. Attach to a MainMenu GameObject in your menu scene.
///
/// SCENE SETUP:
///   1. Create a new scene: File > New Scene
///   2. Add a UI Canvas (GameObject > UI > Canvas)
///   3. Add a Panel to the Canvas for the background
///   4. Add a TextMeshPro Text for the game title
///   5. Add two Buttons: Play and Quit
///   6. Create an empty GameObject called "MainMenu" and attach this script
///   7. Wire up:
///        - Fade Panel     → a full-screen black Image on the Canvas
///        - Play Button    → the Play button
///        - Quit Button    → the Quit button
///        - Game Scene Name → the exact name of the scene to load (no path, no .unity)
///   8. On the Play button's OnClick, call MainMenuManager.OnPlayClicked()
///   9. On the Quit button's OnClick, call MainMenuManager.OnQuitClicked()
///  10. Add this scene AND the game scene to File > Build Settings
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene To Load")]
    [Tooltip("Exact scene name to load when Play is pressed (no path, no .unity extension).")]
    public int sceneID = 1;

    [Header("UI References")]
    [Tooltip("A full-screen black Image used to fade in/out. Set its alpha to 0 in the Inspector.")]
    public Image fadePanel;

    [Tooltip("How long the fade-in takes when the menu first appears.")]
    public float fadeInDuration = 1f;

    [Tooltip("How long the fade-out takes before loading the game.")]
    public float fadeOutDuration = 1f;

    private void Start()
    {
        // Unlock cursor for menu navigation
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fade the menu in from black
        if (fadePanel != null)
            StartCoroutine(FadeIn());
    }

    // ── Button callbacks ──────────────────────────────────────────────────

    public void OnPlayClicked()
    {
        StartCoroutine(LoadGameScene());
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Fading ────────────────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        SetFadeAlpha(1f);
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(1f - Mathf.Clamp01(elapsed / fadeInDuration));
            yield return null;
        }
        SetFadeAlpha(0f);
    }

    private IEnumerator LoadGameScene()
    {
        if (fadePanel != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                SetFadeAlpha(Mathf.Clamp01(elapsed / fadeOutDuration));
                yield return null;
            }
            SetFadeAlpha(1f);
        }

        SceneManager.LoadScene(sceneID);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadePanel == null) return;
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;
    }
}
