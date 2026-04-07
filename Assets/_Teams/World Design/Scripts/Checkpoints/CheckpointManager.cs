using UnityEngine;

namespace _Teams.World_Design.Scripts.Checkpoints
{
    public class CheckpointManager : MonoBehaviour
    {
        [SerializeField] private Checkpoint currentCheckpoint;
        [SerializeField] private Transform boatResetObject;
        [SerializeField] private Transform playerResetObject;

        [SerializeField] private Transform currentPlayerResetPoint;
        [SerializeField] private Transform currentBoatResetPoint;
        
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