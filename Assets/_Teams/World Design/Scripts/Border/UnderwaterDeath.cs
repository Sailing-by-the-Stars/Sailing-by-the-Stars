using _Teams.World_Design.Scripts.Checkpoints;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Border
{
    public class UnderwaterDeath : MonoBehaviour
    {
        
        [SerializeField] private GameObject player; 
        [SerializeField] private float deathHeight = -10f; 

        [SerializeField] private CheckpointManager checkpointManager;
        
        
        
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

        // Update is called once per frame
        void Update()
        {
            Debug.Log(player.transform.position);
            if(player.transform.position.y < deathHeight)
            {
                checkpointManager.GoToCheckpointDrown();
            }
        }
    }
}
