using UnityEngine;

namespace _Teams.World_Design.Scripts.Checkpoints
{
    public class Checkpoint: MonoBehaviour
    {
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private CheckpointManager controller;
        [SerializeField] private Transform boatResetPoint;
        [SerializeField] private Transform playerResetPoint;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(instigatorTag))
            {
                return;
            }
            
            controller.SetCheckpoint(this);
            controller.SetBoatResetPoint(boatResetPoint);
            controller.SetPlayerResetPoint(playerResetPoint);
            Debug.Log("Checkpoint reached: " + gameObject.name);
        }
        
    }
}