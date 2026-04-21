using System.Collections;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    private ScreenEffects effects;
    private Coroutine coroutine;

    private void Start()
    {
        effects = FindFirstObjectByType<ScreenEffects>();
    }
    public void PlayDeathSequence()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(DeathSequence());
    }

    public IEnumerator DeathSequence()
    {
        if (effects == null) yield break;

        //Strong initial blow
        effects.ScreenShake(0.6f, 0.3f);
        effects.Flash(new Color(1f, 1f, 1f, 0.8f), 0.2f); //white flash

        yield return new WaitForSeconds(0.2f);

        //Red effect
        effects.ScreenShake(0.3f, 0.4f);
        effects.Flash(new Color(1f, 0f, 0f, 0.6f), 0.6f);

        yield return new WaitForSeconds(0.3f);

        //Fading
        effects.Vignette(Color.black, 0.8f, 2f);

        yield return new WaitForSeconds(1f);

        //A small final tremor
        effects.ScreenShake(0.1f, 0.5f);
    }
}
