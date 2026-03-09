// created by Christina

using UnityEngine;

public class ZoneSpawner : MonoBehaviour, IZoneEffect
{
    [SerializeField] private GameObject spawnPrefab;
    [Tooltip("If assigned, spawns at this position instead of calculating offset from player")]
    [SerializeField] private Transform spawnAnchor;
    [Tooltip("Distance from player in diagonal towards zone center the object spawns (x, z)")]
    [SerializeField] private float spawnOffsetToCenter = 20f;
    [SerializeField] private float defaultSpawnY = 0f;

    private GameObject spawnedObject;
    private IWorldSpawnable cachedSpawnable;

    public void OnEnter(GameObject instigator)
    {
        if (spawnPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: no spawn prefab assigned");
            return;
        }

        if (spawnedObject != null)
        {
            return;
        }

        Vector3 spawnPoint;
        if (spawnAnchor != null)
        {
            spawnPoint = spawnAnchor.position;
        }
        else
        {
            // flatten to XZ for distance calculation
            Vector3 dirToCenter = transform.position - instigator.transform.position;
            dirToCenter.y = 0f;
            dirToCenter.Normalize();

            spawnPoint = instigator.transform.position + dirToCenter * spawnOffsetToCenter;
            spawnPoint.y = defaultSpawnY;
        }

        spawnedObject = Instantiate(spawnPrefab, spawnPoint, Quaternion.identity);
        cachedSpawnable = spawnedObject.GetComponent<IWorldSpawnable>();

        if (cachedSpawnable != null)
        {
            cachedSpawnable.Initialize(instigator, transform.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: spawn prefab has no IWorldSpawnable");
        }
    }

    public void OnExit(GameObject instigator)
    {
        if (spawnedObject == null)
        {
            return;
        }

        if (cachedSpawnable != null)
        {
            cachedSpawnable.Despawn();
        }
        else
        {
            // fallback for prefabs without IWorldSpawnable — destroy directly
            Destroy(spawnedObject);
        }

        spawnedObject = null;
        cachedSpawnable = null;
    }
}