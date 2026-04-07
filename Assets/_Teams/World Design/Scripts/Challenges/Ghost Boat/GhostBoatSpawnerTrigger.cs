using System.Collections;
using FMODUnity;
using UnityEngine;

namespace _Teams.World_Design.Scripts.Challenges.Ghost_Boat
{
    public class GhostBoatSpawnerTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string instigatorTag = "Player";
        [SerializeField] private GameObject ghostBoatPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool spawnOnlyOnce = true;

        private GameObject spawnedGhost;

        public void OnTriggerEnter(Collider other)
        {
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
    }
}