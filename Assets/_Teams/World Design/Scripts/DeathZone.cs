using UnityEngine;

namespace _Teams.World_Design.Scripts.ZoneEffects.DeathZone
{
    public class DeathZone : MonoBehaviour
    {
        [SerializeField] private string instigatorTag;

        public void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(instigatorTag))
            {
                return;
            }
            
            // Here should be reset to checkpoint code
            Debug.Log("Player Died");
        }

        public void OnTriggerExit(Collider other)
        {
            Debug.Log("No one can escape death");
        }
    }
}