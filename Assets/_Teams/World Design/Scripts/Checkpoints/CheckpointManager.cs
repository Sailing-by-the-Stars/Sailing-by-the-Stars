using System;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Checkpoints
{
    public class CheckpointManager : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField] private Checkpoint currentCheckpoint;
        [SerializeField] private bool anchorBoatOnRespawn = true;

        [Header("Reset Objects")]
        [SerializeField] private string playerResetObjectTag = "Player";
        
        [SerializeField] private GameObject playerResetObject;
        [SerializeField] private Transform boatResetObject;

        
        [Header("Reset Points")]
        [SerializeField] private Transform currentPlayerResetPoint;
        [SerializeField] private Transform currentBoatResetPoint;
        
        private DeathEffect deathEffect;
        private DrownEffect drownEffect;
        private Rigidbody boatRb;
        private BoatController boatController;

        private void Awake()
        {
            if (playerResetObject == null)
            {
                playerResetObject = GameObject.FindWithTag(playerResetObjectTag);
            }
            
            if (boatResetObject == null)
            {
                boatResetObject = FindFirstObjectByType<BoatController>().transform;
            }
            if (boatResetObject != null)
            {
                boatRb = boatResetObject.GetComponent<Rigidbody>();
                boatController = boatResetObject.GetComponent<BoatController>();
            }
            
            deathEffect = FindFirstObjectByType<DeathEffect>();
            if (deathEffect == null)
            {
                Debug.LogWarning("DeathEffect reference is missing in CheckpointManager. Attempting to find one in the scene.");
            }
            drownEffect = FindFirstObjectByType<DrownEffect>();
            if (drownEffect == null)
            {
                Debug.LogWarning("DrownEffect reference is missing in CheckpointManager. Attempting to find one in the scene.");
            }

        }

        public void GoToCheckpointDeath()
        {
            if (currentCheckpoint == null)
            {
                return;
            }

            HandleCheckpointTeleport();

            if (deathEffect)
            {
                drownEffect.PlayFadeOut();
                // deathEffect.PlayDeathSequence();
            }
        }
        
        public void GoToCheckpointDrown()
        {
            if (currentCheckpoint == null)
            {
                return;
            }
            
            HandleCheckpointTeleport();

            if (drownEffect)
            {
                drownEffect.PlayFadeOut();
            }

        }


        private void HandleCheckpointTeleport()
        {
            if (playerResetObject != null
                && currentPlayerResetPoint != null
                && !IsPlayerInsideBoatResetObject())
            {
                Debug.Log($"Going to checkpoint {currentCheckpoint.name}");
                playerResetObject.GetComponent<Rigidbody>().position = currentPlayerResetPoint.position;
                playerResetObject.GetComponent<Rigidbody>().rotation = currentPlayerResetPoint.rotation;
            }

            if (boatResetObject != null && currentBoatResetPoint != null)
            {
                // reset velocity and anchor
                if (boatRb != null)
                {
                    boatRb.linearVelocity = Vector3.zero;
                    boatRb.angularVelocity = Vector3.zero;
                }
                if (boatController != null && anchorBoatOnRespawn)
                {
                    // boatController.DropAnchor();
                }
            
            
                boatResetObject.position = currentBoatResetPoint.position;
                boatResetObject.rotation = currentBoatResetPoint.rotation;
            }
        }

        private bool IsPlayerInsideBoatResetObject()
        {
            return playerResetObject.transform.IsChildOf(boatResetObject);

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