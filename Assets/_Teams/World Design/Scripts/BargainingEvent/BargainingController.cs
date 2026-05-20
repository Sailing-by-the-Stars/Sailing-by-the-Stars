/*
 * Created by Christina Pence
 * Contributed to by:
 */
using System.Collections;
using UnityEngine;
using FMODUnity;

/// <summary>
/// Controls the Bargaining grief event.
/// Spawns a single BargainingEntity instance for duration of the event and destroys when player leaves zone.
///   
/// NOTE: For now assumes player will be parented to boat when sailing in water. Use "Player" tag in event zone trigger
///       and do not place trigger in an an area where player will be outside the boat.
/// </summary>

public class BargainingController : MonoBehaviour
{
    [Tooltip("The parent zone GameObject; deactivated on permanent event complete.")]
    [SerializeField] private GameObject zone;

    [Header("Spawning")]
    [SerializeField] private GameObject entityPrefab;
    [Tooltip("Radius around the boat at which the entity appears.")]
    [SerializeField, Min(0.1f)] private float spawnRadius = 10f;
    [Tooltip("Excluded arc behind boat from spawning in degrees.")]
    [SerializeField, Range(0f, 359f)] private float rearDeadzone = 0f;
    [Tooltip("Delay in seconds before the entity first appears after entering the zone.")]
    [SerializeField] private float firstSpawnDelay = 3f;
    [Tooltip("Time in seconds before entity reappears after dissolving.")]
    [SerializeField] private float reappearanceCooldown = 8f;

    [Header("Audio")]
    [Tooltip("Sound played at entity position when it dissolves.")]
    [SerializeField] private EventReference dissolveSound;
    [Tooltip("Sound played at spawn position when entity reappears.")]
    [SerializeField] private EventReference spawnSound;
    [Tooltip("Sound played before reappearance.")]
    [SerializeField] private EventReference approachingSound;
    [Tooltip("How long before entity spawns the approaching sound should play")]
    [SerializeField] private float approachSoundLeadTime = 2f;

    // State
    private bool eventCompleted = false;
    private bool eventActive = false;
    private BargainingEntity activeEntity = null;
    private GameObject eventTarget = null;
    private Coroutine spawnRoutine = null;

    // TODO: confirm event final end condition if any
    public bool IsCompleted => eventCompleted;
    public bool IsActive => eventActive;

    /// <summary>
    /// Called by the entry zone trigger.
    /// </summary>
    public void StartEvent(GameObject instigator)
    {
        if (eventCompleted || eventActive)
        {
            return;
        }
        BoatController boat = instigator.GetComponentInParent<BoatController>();
        // fallback to event trigger target
        eventTarget = boat != null ? boat.gameObject : instigator;

        eventActive = true;
        spawnRoutine = StartCoroutine(FirstSpawnRoutine());
    }
    /// <summary>
    /// Called when player exits the zone.
    /// Cleans up and resets: does not permanently complete the event.
    /// </summary>
    public void EndEvent()
    {
        if (!eventActive)
        {
            return;
        }

        eventActive = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        if (activeEntity != null)
        {
            activeEntity.OnDissolveComplete -= OnEntityDissolved;
            activeEntity.Despawn();
            activeEntity = null;
        }
    }
    private IEnumerator FirstSpawnRoutine()
    {
        yield return new WaitForSeconds(firstSpawnDelay);
        if (!eventActive || eventCompleted)
        {
            yield break;
        }
        SpawnEntity();
        spawnRoutine = null;
    }
    private void SpawnEntity()
    {
        if (entityPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: No entity prefab assigned.");
            return;
        }
        if (eventTarget == null)
        {
            Debug.LogWarning($"{gameObject.name}: No event target set.");
            return;
        }

        GameObject entityObj = Instantiate(entityPrefab);
        activeEntity = entityObj.GetComponent<BargainingEntity>();

        if (activeEntity == null)
        {
            Debug.LogWarning($"{gameObject.name}: Entity prefab has no BargainingEntity component.");
            Destroy(entityObj);
            eventActive = false;
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        PlaySound(spawnSound, spawnPosition);

        activeEntity.OnDissolveComplete += OnEntityDissolved;
        activeEntity.Initialize(eventTarget, spawnPosition);
    }
    private void OnEntityDissolved()
    {
        if (!eventActive || eventCompleted)
        {
            return;
        }

        if (activeEntity != null)
        {
            PlaySound(dissolveSound, activeEntity.transform.position);
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
        spawnRoutine = StartCoroutine(ReappearanceRoutine());
    }
    private IEnumerator ReappearanceRoutine()
    {
        if (!eventActive || eventCompleted)
        {
            yield break;
        }

        float waitBeforeSound = reappearanceCooldown - approachSoundLeadTime;
        if (waitBeforeSound > 0f)
        {
            yield return new WaitForSeconds(waitBeforeSound);
        }

        if (!eventActive || eventCompleted)
        {
            yield break;
        }

        PlaySound(approachingSound, eventTarget.transform.position);

        yield return new WaitForSeconds(approachSoundLeadTime);

        if (!eventActive || eventCompleted)
        {
            yield break;
        }
        if (activeEntity != null)
        {
            Vector3 newPosition = GetSpawnPosition();
            PlaySound(spawnSound, newPosition);
            activeEntity.Reappear(newPosition);
        }

        spawnRoutine = null;
    }
    private Vector3 GetSpawnPosition()
    {
        // Spawn anywhere in the forward arc, excluding the rear deadzone
        float halfArc = 180f - (rearDeadzone / 2f);
        float randomAngle = Random.Range(-halfArc, halfArc);
        float boatYaw = eventTarget.transform.eulerAngles.y;
        float finalAngle = (boatYaw + randomAngle) * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Sin(finalAngle), 0f, Mathf.Cos(finalAngle)) * spawnRadius;
        return eventTarget.transform.position + offset;
    }
    private void PlaySound(EventReference sound, Vector3 position)
    {
        if (!sound.IsNull)
        {
            RuntimeManager.PlayOneShot(sound, position);
        }
    }
    private void CompleteEvent()
    {
        eventCompleted = true;
        EndEvent();

        if (zone != null)
        {
            zone.SetActive(false);
        }
    }
#if UNITY_EDITOR
 private void OnDrawGizmos()
    {
        Vector3 center = eventTarget != null ? eventTarget.transform.position : transform.position;
        float yaw = eventTarget != null ? eventTarget.transform.eulerAngles.y : 0f;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center, spawnRadius);

        // Draw rear deadzone edges
        Gizmos.color = Color.red;
        float leftAngle = (yaw + 180f - rearDeadzone / 2f) * Mathf.Deg2Rad;
        float rightAngle = (yaw + 180f + rearDeadzone / 2f) * Mathf.Deg2Rad;
        Gizmos.DrawLine(center, center + new Vector3(Mathf.Sin(leftAngle), 0f, Mathf.Cos(leftAngle)) * spawnRadius);
        Gizmos.DrawLine(center, center + new Vector3(Mathf.Sin(rightAngle), 0f, Mathf.Cos(rightAngle)) * spawnRadius);
    }
    private void OnValidate()
    {
        approachSoundLeadTime = Mathf.Clamp(approachSoundLeadTime, 0f, reappearanceCooldown);
    }
#endif

}