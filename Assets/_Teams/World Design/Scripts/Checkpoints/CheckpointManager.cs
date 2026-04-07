using System;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Checkpoints
{
    public class CheckpointManager : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField] private Checkpoint currentCheckpoint;
        
        [Header("Reset Objects")]
        [SerializeField] private Transform boatResetObject;
        [SerializeField] private Transform playerResetObject;

        [Header("Reset Points")]
        [SerializeField] private Transform currentPlayerResetPoint;
        [SerializeField] private Transform currentBoatResetPoint;
        
        private DeathEffect deathEffect;

        private void Awake()
        {
            if (boatResetObject == null)
            {
                Debug.LogWarning($"{nameof(CheckpointManager)} is missing a reference to the boat reset object.", this);
            }
            
            if (playerResetObject == null)
            {
                Debug.LogWarning($"{nameof(CheckpointManager)} is missing a reference to the player reset object.", this);
            }

            deathEffect = FindFirstObjectByType<DeathEffect>();
        }

        public void GoToCheckpoint()
        {
            if (currentCheckpoint == null)
            {
                return;
            }

            if (playerResetObject != null
                && currentPlayerResetPoint != null
                && !IsPlayerInsideBoatResetObject())
            {
                Debug.Log($"Going to checkpoint {currentCheckpoint.name}");
                playerResetObject.position = currentPlayerResetPoint.position;
                playerResetObject.rotation = currentPlayerResetPoint.rotation;
            }
            
            if (boatResetObject != null && currentBoatResetPoint != null)
            {
                boatResetObject.position = currentBoatResetPoint.position;
                boatResetObject.rotation = currentBoatResetPoint.rotation;
            }
            
            StartCoroutine(deathEffect.DeathSequence());
        }

        private bool IsPlayerInsideBoatResetObject()
        {
            return playerResetObject.IsChildOf(boatResetObject);
            
        }

        public void SetCheckpoint(Checkpoint checkpoint)
        {
            currentCheckpoint = checkpoint;
        }
        
        public void SetBoatResetPoint(Transform resetPoint)
        {
            currentBoatResetPoint = resetPoint;
        }
        
        public void SetPlayerResetPoint(Transform resetPoint)
        {
            currentPlayerResetPoint = resetPoint;
        }
    }
}