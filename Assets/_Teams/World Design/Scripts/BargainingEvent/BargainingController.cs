/*
 * Created by Christina Pence
 * Contributed to by:
 */
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the Bargaining grief event.
/// Owns event state, timer countdown, checkpoint sequencing, entity spawning and despawning.
///
/// SETUP:
///   Add this component to a GameObject in the scene
///   Assign entity prefab (BargainingEntity component required, all renderers disabled)
///   
/// MODE: OrderedCheckpoints
///   Add BargainingCheckpoint components to trigger collider GameObjects in sequence
///      -- assign them to the checkpoints list in order
///      -- tick isFinalCheckpoint on the last one
///   Add BargainingZoneEffect alongside WorldEventZone on the entry trigger
/// 
/// MODE: StarEvents
///     Subscribes to TwinklingStar event to trigger cooldown.
///     Order of reaching the checkpoints does not matter.
///     Event ends permanent with distance check (assign transform in inspector)
///     
/// NOTE: For now assumes player will be parented to boat when sailing in water. Use "Player" tag in event zone trigger
///       and do not place trigger in an an area where player will be outside the boat.
///
/// FLOW
///   Entry zone triggered: entity spawns, timer starts immediately
///   Entity spirals inward: visual pressure builds
///   (OrderedCheckpoints) Checkpoint reached: entity despawned, timer paused, cooldown, entity respawns, timer resets
///   (Ordered Checkpoints) Final checkpoint reached: event completed permanently
///   (StarEvents) Star found: entity despawned, cooldown, entity respawns, timer resets
///   (StarEvents) Event ends permanently by distance check.
///   Timer reaches 0: kill effects, player teleported to respawn point, event resets
/// </summary>

public enum BargainingEventMode
{
    OrderedCheckpoints,
    StarEvents
}
public class BargainingController : MonoBehaviour
{
    [Header("Mode Setup")]
    [SerializeField] private BargainingEventMode eventMode;
    [Tooltip("Ordered list of checkpoint for OrderedCheckpoints Mode" +
         "Tick isFinalCheckpoint on the last one.")]
    [SerializeField] private BargainingCheckpoint[] checkpoints;
    [Tooltip("Transform to measure distance from for end condition in StarEvents mode")]
    [SerializeField] private Transform endConditionTransform;
    [Tooltip("Distance threshold to end the event from transform when using distance end condition")]
    [SerializeField] private float endConditionDistance = 30f;

    [Header("Entity")]
    [Tooltip("Prefab with BargainingEntity component. Disable all renderers on the prefab.")]
    [SerializeField] private GameObject entityPrefab;

    [Header("Timing")]
    [Tooltip("How long the player has to reach a checkpoint." +
        "Can be overridden per checkpoint (OrderedCheckpoint mode only)")]
    [SerializeField] private float defaultTimerDuration = 120f;

    [Tooltip("Default cooldown after checkpoint reached before entity returns. " +
             "Can be overridden per checkpoint (OrderedCheckpoint mode only)")]
    [SerializeField] private float defaultCooldownDuration = 30f;

    [Tooltip("Position to teleport player to on timer fail. Place outside event zone.")]
    [SerializeField] private Transform respawnPoint;

    // State
    private bool eventCompleted = false;
    private bool eventActive = false;
    private int currentCheckpointIndex = 0;
    private float timerRemaining = 0f;
    private float currentTimerDuration = 0f; // tracks active duration for audio intensity calculation
    private BargainingEntity activeEntity;
    private GameObject eventTarget;
    private Coroutine cooldownRoutine;

    // respawn visual
    private ScreenEffects screenEffects;

    // audio
    private SetBargainingTimer timerAudio;
    [Tooltip("How much to increase the audio intensity when audio is active")]
    [SerializeField] private float audioBoost = 0.2f;
    private float TimerIntensity => eventActive ? 1f - Mathf.Clamp01(timerRemaining / currentTimerDuration) : 0f;

    public bool IsCompleted => eventCompleted;
    public bool IsActive => eventActive;

    private void Start()
    {
        timerAudio = FindFirstObjectByType<SetBargainingTimer>();
        screenEffects = FindFirstObjectByType<ScreenEffects>();

        if (eventMode == BargainingEventMode.StarEvents && endConditionTransform == null)
        {
            Debug.LogWarning($"{gameObject.name}: StarEvents mode requires endConditionTransform to be assigned.");
        }

    }
    private void OnEnable()
    {
        if (eventMode == BargainingEventMode.StarEvents)
        {
            TwinklingStar.OnStarFound += HandleStarFound;
        }
    }
    private void OnDisable()
    {
        if (eventMode == BargainingEventMode.StarEvents)
        {
            TwinklingStar.OnStarFound -= HandleStarFound;
        }
    }

    private void Update()
    {
        if (!eventActive || eventCompleted)
        {
            return;
        }
        timerRemaining -= Time.deltaTime;
        // mode specific distance check to endpoint
        if (eventMode == BargainingEventMode.StarEvents)
        {
            CheckEndConditionDistance();
        }

        if (timerAudio != null)
        {
            float targetAudioIntensity = TimerIntensity;
            if (TimerIntensity != 0f && audioBoost != 0f)
            {
                targetAudioIntensity = TimerIntensity + audioBoost;
            }
            timerAudio.SetIntensity(targetAudioIntensity);
        }
        // Trigger warning effects if desired here

        if (timerRemaining <= 0f)
        {
            eventActive = false; // stop timer immediately to prevent multiple coroutine starts
            StartCoroutine(HandleTimerFail());
        }
    }
    /// <summary>
    /// Spawns entity and starts timer.
    /// </summary>
    public void StartEvent(GameObject instigator)
    {
        if (eventCompleted || eventActive)
        {
            return;
        }
        // assumes boat controller script will be on the teleport target (player parented to boat)
        BoatController boat = instigator.GetComponentInParent<BoatController>();
        eventTarget = boat != null ? boat.gameObject : instigator;

        // Checkpoint mode specific logic
        if (eventMode == BargainingEventMode.OrderedCheckpoints)
        {
            currentCheckpointIndex = 0;
            ActivateCurrentCheckpoint();
        }

        timerRemaining = defaultTimerDuration;
        currentTimerDuration = defaultTimerDuration;

        SpawnEntity(defaultTimerDuration);
        eventActive = true;
    }

    /// <summary>
    /// Applies checkpoint overrides and starts cooldown or completes event for final checkpoint.
    /// </summary>
    public void OnCheckpointReached(BargainingCheckpoint checkpoint)
    {
        if (!eventActive || eventMode != BargainingEventMode.OrderedCheckpoints)
        {
            return;
        }

        if (checkpoint.IsFinalCheckpoint)
        {
            CompleteEvent();
            return;
        }

        float cooldown = checkpoint.CooldownOverride > 0f ? checkpoint.CooldownOverride : defaultCooldownDuration;
        float timerDuration = checkpoint.TimerOverride > 0f ? checkpoint.TimerOverride : defaultTimerDuration;
        
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }
        cooldownRoutine = StartCoroutine(CheckpointCooldownRoutine(cooldown, timerDuration));
    }
    /// <summary>
    /// Called when a star is found. Despawns entity and starts cooldown
    /// </summary>
    private void HandleStarFound()
    {
        if (!eventActive || eventMode != BargainingEventMode.StarEvents)
        {
            return;
        }
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }
        cooldownRoutine = StartCoroutine(CheckpointCooldownRoutine(defaultCooldownDuration, defaultTimerDuration));
    }
    private void CheckEndConditionDistance()
    {
        if (endConditionTransform == null || eventTarget == null)
        {
            return;
        }
        float distance = Vector3.Distance(eventTarget.transform.position, endConditionTransform.position);
        if (distance <= endConditionDistance)
        {
            CompleteEvent();
        }
    }
    private void SpawnEntity(float timerDuration)
    {
        if (entityPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} No entity prefab assigned.");
            return;
        }
        
        GameObject entityObj = Instantiate(entityPrefab, eventTarget.transform.position, Quaternion.identity);
        activeEntity = entityObj.GetComponent<BargainingEntity>();

        if (activeEntity == null)
        {
            Debug.LogWarning($"{gameObject.name} Entity prefab has no BargainingEntity component.");
            return;
        }

        activeEntity.Initialize(eventTarget, timerDuration);
    }
    private void DespawnEntity()
    {
        if (activeEntity == null)
        {
            return;
        }
        activeEntity.Despawn();
        activeEntity = null;
    }
    private void ActivateCurrentCheckpoint()
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i]?.SetActive(i == currentCheckpointIndex);
        }
    }
    private IEnumerator CheckpointCooldownRoutine(float cooldown, float nextTimerDuration)
    {
        if (eventMode == BargainingEventMode.OrderedCheckpoints)
        {
            currentCheckpointIndex++;
            ActivateCurrentCheckpoint();
        }

        DespawnEntity();
        eventActive = false; // pause timer during cooldown

        if (timerAudio != null)
        {
            timerAudio.ResetIntensity();
        }    

        yield return new WaitForSeconds(cooldown);

        timerRemaining = nextTimerDuration;
        currentTimerDuration = nextTimerDuration;

        SpawnEntity(nextTimerDuration);
        eventActive = true;
    }
    private IEnumerator HandleTimerFail()
    {
        DespawnEntity();
        if (timerAudio != null)
        {
            timerAudio.ResetIntensity();
        }
        // visuals
        yield return StartCoroutine(TimerFailRoutine());

        if (respawnPoint != null && eventTarget != null)
        {
            eventTarget.transform.position = respawnPoint.position;
        }
        // reset event so it can trigger again
        if (eventMode == BargainingEventMode.OrderedCheckpoints)
        {
            currentCheckpointIndex = 0;
            ActivateCurrentCheckpoint();
        }
    }
    private void CompleteEvent()
    {
        Debug.Log("Event ended");
        eventCompleted = true;
        eventActive = false;
        DespawnEntity();

        if (timerAudio != null)
        {
            timerAudio.ResetIntensity();
        }

        if (eventMode == BargainingEventMode.OrderedCheckpoints)
        {
            foreach (BargainingCheckpoint checkpoint in checkpoints)
            {
                if (checkpoint != null)
                {
                    checkpoint.SetActive(false);
                }
            }
        }
    }

    // TODO: replace with final visuals or sequence from screen effects
    private IEnumerator TimerFailRoutine()
    {
        Debug.Log("Event failed");
        if (screenEffects == null)
        {
            yield break;
        }
        screenEffects.ScreenShake(magnitude: 0.5f, duration: 1.2f);
        yield return new WaitForSeconds(0.7f);
        screenEffects.Vignette(Color.black, alpha: 0.6f, duration: 1.5f);
        yield return new WaitForSeconds(1.5f);
        screenEffects.Flash(Color.darkRed, duration: 1f);
        yield return new WaitForSeconds(0.5f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (eventMode != BargainingEventMode.StarEvents)
        {
            return;
        }
        if (endConditionTransform == null)
        {
            return;
        }
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(endConditionTransform.position, endConditionDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(endConditionTransform.position, endConditionDistance);
    }
#endif
}