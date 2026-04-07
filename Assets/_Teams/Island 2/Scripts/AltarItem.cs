using UnityEngine;

public class AltarItem : MonoBehaviour
{
    public enum ItemType
    {
        Career,
        Passion,
        Love,
        Wealth,
        Health
    }

    [SerializeField] private ItemType itemType;
    [SerializeField] private float weight;

    public ItemType Type => itemType;
    public float Weight => weight;

    private void Start()
    {
        EnsureCollider();
    }

    private void EnsureCollider()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            Debug.Log($"Added SphereCollider to {gameObject.name}");
        }
    }

    public void SetItemType(ItemType type)
    {
        itemType = type;
    }
}
