using UnityEngine;
using System.Collections;

/// <summary>
/// Code by Alonso
/// </summary>

public class DrownEffect : MonoBehaviour
{
    private ScreenEffects effects;
    private Coroutine coroutine;

    private void Start()
    {
        effects = FindFirstObjectByType<ScreenEffects>();
    }

    public void PlayFadeOut()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        if (effects == null) yield break;

        effects.Vignette(Color.black, 1f, 2f);

        yield return new WaitForSeconds(2f);
    }
}
