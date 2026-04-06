using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Ghost_Boat
{
    public class GhostBoatDespawnZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private string ghostBoatTag = "GhostBoat";
        
        
        private SetDenialEventMusic soundController;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                soundController = FindFirstObjectByType<SetDenialEventMusic>();
                soundController.SetDenialMusic(0f);
                
                GhostBoatScript[] ghostBoats = FindObjectsOfType<GhostBoatScript>();
                
                foreach (GhostBoatScript ghostBoat in ghostBoats)
                {
                    if (ghostBoat.gameObject.CompareTag(ghostBoatTag))
                    {
                        Destroy(ghostBoat.gameObject);
                    }
                }
                
            }
        }
    }
}

