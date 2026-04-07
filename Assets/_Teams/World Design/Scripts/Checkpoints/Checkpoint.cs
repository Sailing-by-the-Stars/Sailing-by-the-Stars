using UnityEngine;

namespace _Teams.World_Design.Scripts.Checkpoints
{
    public class Checkpoint: MonoBehaviour
    {
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private CheckpointManager checkpointManager;
        [SerializeField] private Transform boatResetPoint;
        [SerializeField] private Transform playerResetPoint;
        
        private void Awake()
        {
            if (checkpointManager == null)
            {
                checkpointManager = FindFirstObjectByType<CheckpointManager>();
                if (checkpointManager == null)
                {
                    Debug.LogWarning($"{nameof(DeathZone)} could not find a {nameof(CheckpointManager)} in the scene.", this);
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(instigatorTag))
            {
                return;
            }
            
            checkpointManager.SetCheckpoint(this);
            checkpointManager.SetBoatResetPoint(boatResetPoint);
            checkpointManager.SetPlayerResetPoint(playerResetPoint);
            Debug.Log("Checkpoint reached: " + gameObject.name);
        }
        
    }
}