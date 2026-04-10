/*
 * Created by Christina Pence
 * Contributed to by:
 */
using UnityEngine;

/// <summary>
/// Checkpoint for the Bargaining grief event.
/// Add a Collider with Is Trigger ticked and a kinematic Rigidbody to the same GameObject.
///
/// Regular checkpoints temporarily dismiss the entity with a cooldown.
/// Final checkpoint completes the event permanently.
///
/// Checkpoints are ordered: BargainingController activates them in sequence.
/// Only the currently active checkpoint responds to triggers.
/// </summary>
public class BargainingCheckpoint : MonoBehaviour
{
    [Tooltip("If ticked, reaching this checkpoint completes the event permanently.")]
    [SerializeField] private bool isFinalCheckpoint = false;

    [Tooltip("Override the default cooldown duration from BargainingController. " +
             "Leave at 0 to use the controller default.")]
    [SerializeField] private float cooldownOverride = 0f;

    [Tooltip("Override the timer reset duration from BargainingController. " +
             "Leave at 0 to use the controller default.")]
    [SerializeField] private float timerOverride = 0f;

    [Tooltip("Tag of the object that triggers this checkpoint.")]
    [SerializeField] private string instigatorTag = "boat";

    private BargainingCheckpointVisual visual;

    public bool IsFinalCheckpoint => isFinalCheckpoint;
    public float CooldownOverride => cooldownOverride;
    public float TimerOverride => timerOverride;

    private BargainingController controller;
    private bool isActive = false;

    private void Start()
    {
        controller = FindFirstObjectByType<BargainingController>();
        if (controller == null)
        {
            Debug.LogWarning($"{gameObject.name}: No BargainingController found in scene.");
        }
        visual = GetComponent<BargainingCheckpointVisual>();
    }

    /// <summary>
    /// Activate this checkpoint. Only the active checkpoint responds to triggers.
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        if (visual != null)
        {
            visual.SetVisible(active);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || controller == null)
        {
            return;
        }
        if (!other.CompareTag(instigatorTag))
        {
            return;
        }
        controller.OnCheckpointReached(this);
    }
}