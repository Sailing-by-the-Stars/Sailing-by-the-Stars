using System.Collections;
using FMODUnity;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Ghost_Boat
{
    public class GhostBoatZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private string ghostBoatTag = "GhostBoat";
        [SerializeField] private GameObject ghostBoatPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool spawnOnlyOnce = true;
        [SerializeField, Range(0, 1)] private float musicIntensity= 0.5f;
        

        private GameObject spawnedGhost;
        private SetDenialEventMusic soundController;
        
        public void OnTriggerEnter(Collider other)
        {
            soundController = FindFirstObjectByType<SetDenialEventMusic>();
            if (!other.CompareTag(instigatorTag))
            {
                return;
            }

            if (ghostBoatPrefab == null)
            {
                Debug.LogWarning("Ghost boat prefab is not assigned.", this);
                return;
            }

            if (spawnOnlyOnce && spawnedGhost != null)
            {
                return;
            }

            if (soundController != null)
            {
                soundController.SetDenialMusic(musicIntensity);
            }

            Transform point = spawnPoint != null ? spawnPoint : transform;
            spawnedGhost = Instantiate(ghostBoatPrefab, point.position, point.rotation);

            if (spawnedGhost.TryGetComponent(out GhostBoatScript ghostBoatScript))
            {
                ghostBoatScript.SetTarget(other.transform);
            }
            else
            {
                Debug.LogWarning("Spawned ghost object has no GhostBoatScript component.", spawnedGhost);
            }

        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(instigatorTag))
            {
                soundController = FindFirstObjectByType<SetDenialEventMusic>();
                if (soundController != null)
                {
                    soundController.SetDenialMusic(0f);
                }

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