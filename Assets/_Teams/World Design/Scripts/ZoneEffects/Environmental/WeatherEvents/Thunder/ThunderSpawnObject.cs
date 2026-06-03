using UnityEngine;

namespace _Teams.World_Design.Scripts.ZoneEffects.Environmental.WeatherEvents.Thunder
{
    public class ThunderSpawnObject : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private GameObject spawnPrefab;
        [SerializeField] private bool spawnOnStart = false;
        [SerializeField] private Vector3 worldOffset;
        [SerializeField] private float intensity;
        [SerializeField] private GameObject boundsObject;

        private GameObject lastSpawnedInstance;

        private void Start()
        {
            if (boundsObject == null && transform.childCount > 0)
            {
                boundsObject = transform.GetChild(0).gameObject;
            }

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

            Vector3 spawnPosition = GetBottomRandomPosition() + worldOffset; 
            Quaternion spawnRotation = Quaternion.identity;

            lastSpawnedInstance = Instantiate(spawnPrefab, spawnPosition, spawnRotation, transform);
            
            return lastSpawnedInstance;
        }

        private Vector3 GetCenterPosition()
        {
            Bounds bounds = ResolveBounds();
            return bounds.center;
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
            GameObject targetObject = boundsObject != null ? boundsObject : gameObject;
            
            if (targetObject.TryGetComponent(out Collider objectCollider))
            {
                return objectCollider.bounds;
            }

            if (targetObject.TryGetComponent(out Renderer objectRenderer))
            {
                return objectRenderer.bounds;
            }

            return new Bounds(targetObject.transform.position, Vector3.zero);
        }
    }
}