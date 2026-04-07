using UnityEngine;

public class PedestalOld : MonoBehaviour
{
    private AltarItem placedItem;
    private int pedestalIndex;

    public AltarItem PlacedItem => placedItem;
    public int PedestalIndex => pedestalIndex;

    public void Initialize(int index)
    {
        pedestalIndex = index;
    }

    public bool PlaceItem(AltarItem item)
    {
        if (placedItem != null)
        {
            return false;
        }

        placedItem = item;
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        return true;
    }

    public AltarItem RemoveItem()
    {
        AltarItem item = placedItem;
        if (item != null)
        {
            item.transform.SetParent(null);
        }
        placedItem = null;
        return item;
    }

    public bool IsOccupied => placedItem != null;
}
