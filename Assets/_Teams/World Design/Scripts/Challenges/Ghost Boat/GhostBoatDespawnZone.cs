using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Ghost_Boat
{
    public class GhostBoatDespawnZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private string ghostBoatTag = "GhostBoat";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
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

