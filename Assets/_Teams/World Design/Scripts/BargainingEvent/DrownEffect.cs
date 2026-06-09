using UnityEngine;
using System.Collections;
using _Teams.World_Design.Scripts.Checkpoints;

/// <summary>
/// Code by Alonso
/// </summary>

public class DrownEffect : MonoBehaviour
{
    private ScreenEffects effects;
    public Coroutine coroutine;

    [SerializeField] private CheckpointManager checkpointManager;

    private void Awake()
    {
        if (checkpointManager == null)
        {
            checkpointManager = FindFirstObjectByType<CheckpointManager>();

            if (checkpointManager == null)
            {
                Debug.LogWarning(
                    $"{nameof(Checkpoint)} could not find a {nameof(CheckpointManager)} in the scene.",
                    this
                );
            }
        }
    }

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
        if (effects == null)
            yield break;

        effects.Vignette(Color.black, 0.35f, 0.4f);

        yield return new WaitForSeconds(0.6f);

        effects.Vignette(Color.black, 0f, 0.3f);

        yield return new WaitForSeconds(0.35f);

        effects.Vignette(Color.black, 1f, 1f);

        yield return new WaitForSeconds(1f);

        checkpointManager.HandleCheckpointTeleport();

        coroutine = null;
    }
}
