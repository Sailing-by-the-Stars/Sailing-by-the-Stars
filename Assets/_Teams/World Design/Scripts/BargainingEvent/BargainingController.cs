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
///   Add BargainingCheckpoint components to trigger collider GameObjects in sequence
///      -- assign them to the checkpoints list in order
///      -- tick isFinalCheckpoint on the last one
///   Add BargainingZoneEffect alongside WorldEventZone on the entry trigger
///
/// FLOW:
///   Entry zone triggered: entity spawns, timer starts immediately
///   Entity spirals inward: visual pressure builds
///   Checkpoint reached: entity despawned, timer paused, cooldown, entity respawns, timer resets
///   Final checkpoint reached: event completed permanently
///   Timer reaches 0: kill effects, player teleported to respawn point, event resets
/// </summary>
public class BargainingController : MonoBehaviour
{
    [Header("Entity")]
    [Tooltip("Prefab with BargainingEntity component. Disable all renderers on the prefab.")]
    [SerializeField] private GameObject entityPrefab;

    [Header("Timing")]
    [Tooltip("How long the player has to reach a checkpoint. Can be overridden per checkpoint.")]
    [SerializeField] private float defaultTimerDuration = 120f;

    [Tooltip("Default cooldown after checkpoint reached before entity returns. " +
             "Can be overridden per checkpoint.")]
    [SerializeField] private float defaultCooldownDuration = 30f;

    [Header("Checkpoints")]
    [Tooltip("Ordered list of checkpoints. Only one active at a time. " +
             "Tick isFinalCheckpoint on the last one.")]
    [SerializeField] private BargainingCheckpoint[] checkpoints;

    [Tooltip("Position to teleport player to on timer fail. Place outside event zone.")]
    [SerializeField] private Transform respawnPoint;

    // State
    private bool eventCompleted = false;
    private bool eventActive = false;
    private int currentCheckpointIndex = 0;
    private float timerRemaining = 0f;
    private BargainingEntity activeEntity;
    private GameObject boat;

    public bool IsCompleted => eventCompleted;
    public bool IsActive => eventActive;

    private void Update()
    {
        if (!eventActive || eventCompleted)
        {
            return;
        }

        timerRemaining -= Time.deltaTime;

        // TODO: add warning effect when entity is at minimum radius and timer is low
        // if (activeEntity != null && activeEntity.AtMinimumRadius && timerRemaining < warningThreshold)
        // {
        //     TriggerWarningEffect();
        // }

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

        boat = instigator;
        currentCheckpointIndex = 0;
        ActivateCurrentCheckpoint();
        timerRemaining = defaultTimerDuration;
        SpawnEntity(defaultTimerDuration);
        eventActive = true;
    }

    /// <summary>
    /// Applies checkpoint overrides and starts cooldown or completes event for final checkpoint.
    /// </summary>
    public void OnCheckpointReached(BargainingCheckpoint checkpoint)
    {
        if (!eventActive)
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

        StartCoroutine(CheckpointCooldownRoutine(cooldown, timerDuration));
    }

    private void SpawnEntity(float timerDuration)
    {
        if (entityPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} No entity prefab assigned.");
            return;
        }

        GameObject entityObj = Instantiate(entityPrefab, boat.transform.position, Quaternion.identity);
        activeEntity = entityObj.GetComponent<BargainingEntity>();

        if (activeEntity == null)
        {
            Debug.LogWarning($"{gameObject.name} Entity prefab has no BargainingEntity component.");
            return;
        }

        activeEntity.Initialize(boat, timerDuration);
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
        currentCheckpointIndex++;
        ActivateCurrentCheckpoint();
        DespawnEntity();

        eventActive = false; // pause timer during cooldown

        yield return new WaitForSeconds(cooldown);

        timerRemaining = nextTimerDuration;
        SpawnEntity(nextTimerDuration);
        eventActive = true;
    }
    private IEnumerator HandleTimerFail()
    {
        DespawnEntity();

        // TODO:  kill effects when ready

        yield return new WaitForSeconds(0.5f);

        // TODO: replace with proper teleport from checkpoint system
        if (respawnPoint != null && boat != null)
        {
            boat.transform.position = respawnPoint.position;
        }
        // reset event so it can trigger again
        currentCheckpointIndex = 0;
        ActivateCurrentCheckpoint();
    }
    private void CompleteEvent()
    {
        eventCompleted = true;
        eventActive = false;
        DespawnEntity();

        foreach (BargainingCheckpoint checkpoint in checkpoints)
        {
            if (checkpoint != null)
            {
                checkpoint.SetActive(false);
            }
        }
    }
}