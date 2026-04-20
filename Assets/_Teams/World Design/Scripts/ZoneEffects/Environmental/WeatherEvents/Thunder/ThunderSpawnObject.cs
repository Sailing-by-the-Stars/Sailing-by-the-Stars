using UnityEngine;

namespace _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents.Thunder
{
    public class ThunderSpawnObject : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private GameObject spawnPrefab;
        [SerializeField] private bool spawnOnStart;
        [SerializeField] private Vector3 worldOffset;
        [SerializeField] private float intensity;

        private GameObject lastSpawnedInstance;

        private void Start()
        {
            if (spawnOnStart)
            {
                TriggerSpawn();
            }
        }

        public GameObject TriggerSpawn()
        {
            if (spawnPrefab == null)
            {
                return null;
            }

            Vector3 spawnPosition = GetBottomRandomPosition() ; 
            Quaternion spawnRotation = Quaternion.identity;

            lastSpawnedInstance = Instantiate(spawnPrefab, spawnPosition, spawnRotation);
            Debug.Log($"Spawned {lastSpawnedInstance.transform.name} at {spawnPosition} with intensity {spawnRotation}");
            return lastSpawnedInstance;
        }

        private Vector3 GetBottomRandomPosition()
        {
            Bounds bounds = ResolveBounds();
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
                );
        }

        private Bounds ResolveBounds()
        {
            if (TryGetComponent(out Renderer objectRenderer))
            {
                return objectRenderer.bounds;
            }

            if (TryGetComponent(out Collider objectCollider))
            {
                return objectCollider.bounds;
            }

            return new Bounds(transform.position, Vector3.zero);
        }

        private void OnDrawGizmos()
        {
            Bounds bounds = ResolveBounds();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}