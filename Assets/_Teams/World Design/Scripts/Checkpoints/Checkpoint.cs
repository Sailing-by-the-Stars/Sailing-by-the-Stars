using UnityEngine;

namespace _Teams.World_Design.Scripts.Checkpoints
{
    public class Checkpoint: MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private CheckpointManager checkpointManager;
        
        [Header("Reset Points")]
        [SerializeField] private Transform boatResetPoint;
        [SerializeField] private Transform playerResetPoint;
        
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
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(instigatorTag) || checkpointManager == null)
            {
                return;
            }
            SetAsActiveCheckpoint();
        }
        public void SetAsActiveCheckpoint()
        {
            checkpointManager.SetCheckpoint(this);
            checkpointManager.SetBoatResetPoint(boatResetPoint);
            checkpointManager.SetPlayerResetPoint(playerResetPoint);
        }
    }
}