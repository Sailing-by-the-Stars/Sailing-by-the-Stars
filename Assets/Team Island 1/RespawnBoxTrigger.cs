using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArmRespawnTrigger : MonoBehaviour
{
    [Header("Trigger To Enable")]
    [Tooltip("Drag the red PointOfNoReturn trigger collider here.")]
    public Collider respawnTriggerCollider;

    private bool _armed = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (respawnTriggerCollider != null)
            respawnTriggerCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_armed) return;
        if (!other.CompareTag("Player")) return;

        _armed = true;

        if (respawnTriggerCollider != null)
        {
            respawnTriggerCollider.enabled = true;
            Debug.Log("[ArmRespawnTrigger] Respawn trigger enabled.");
        }
        else
        {
            Debug.LogWarning("[ArmRespawnTrigger] No respawn trigger collider assigned.", this);
        }
    }
}