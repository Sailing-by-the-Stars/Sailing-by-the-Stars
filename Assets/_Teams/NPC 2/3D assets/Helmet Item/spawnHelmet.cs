using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    private bool hasSpawned;

    public void SpawnItem()
    {
        if (hasSpawned || itemPrefab == null)
            return;

        Instantiate(itemPrefab, transform.position, transform.rotation);
        hasSpawned = true;
    }
}