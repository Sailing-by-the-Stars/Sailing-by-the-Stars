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
                Debug.LogWarning($"{nameof(Checkpoint)} could not find a {nameof(CheckpointManager)} in the scene.", this);
            }
        }
    }
    
    private void Start()
    {
        effects = FindFirstObjectByType<ScreenEffects>();
    }

    public void PlayFadeOut()
    {
        coroutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        if (effects == null) yield break;

        effects.Vignette(Color.black, 1f, 2.5f);

        yield return new WaitForSeconds(1f);
        checkpointManager.HandleCheckpointTeleport();
        coroutine = null;
    }
}
