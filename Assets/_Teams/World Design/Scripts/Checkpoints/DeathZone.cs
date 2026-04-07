using UnityEngine;

namespace _Teams.World_Design.Scripts.Checkpoints
{
    public class DeathZone : MonoBehaviour
    {
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private CheckpointManager checkpointManager;
        
        private void Awake()
        {
            if (checkpointManager == null)
            {
                checkpointManager = FindObjectOfType<CheckpointManager>();
                if (checkpointManager == null)
                {
                    Debug.LogWarning($"{nameof(DeathZone)} could not find a {nameof(CheckpointManager)} in the scene.", this);
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(instigatorTag))
            {
                return;
            }

            if (checkpointManager == null)
            {
                return;
            }

            checkpointManager.GoToCheckpoint();
            // Here should be reset to checkpoint code
            Debug.Log("Player Died");
        }

        public void OnTriggerExit(Collider other)
        {
            Debug.Log("No one can escape death");
        }
    }
}