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
        [SerializeField] private String playerResetObjectTag = "Player";
        
        [SerializeField] private Transform playerResetObject;
        [SerializeField] private Transform boatResetObject;

        
        [Header("Reset Points")]
        [SerializeField] private Transform currentPlayerResetPoint;
        [SerializeField] private Transform currentBoatResetPoint;
        
        private DeathEffect deathEffect;
        private Rigidbody boatRb;
        private BoatController boatController;

        private void Awake()
        {
            if (playerResetObject == null)
            {
                playerResetObject = GameObject.FindWithTag(playerResetObjectTag).transform;
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
                // reset velocity and anchor
                if (boatRb != null)
                {
                    boatRb.linearVelocity = Vector3.zero;
                    boatRb.angularVelocity = Vector3.zero;
                }
                if (boatController != null && anchorBoatOnRespawn)
                {
                    boatController.DropAnchor();
                }


                boatResetObject.position = currentBoatResetPoint.position;
                boatResetObject.rotation = currentBoatResetPoint.rotation;
            }

            if (deathEffect != null)
            {
                deathEffect.PlayDeathSequence();
            }
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