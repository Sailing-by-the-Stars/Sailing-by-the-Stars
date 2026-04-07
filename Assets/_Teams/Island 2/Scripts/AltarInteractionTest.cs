using UnityEngine;

public class AltarInteractionTest : MonoBehaviour
{
    [SerializeField] private Altar altar;
    [SerializeField] private AltarItem[] items;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceItemsInCorrectOrder();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            altar.LightCandle();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAltar();
        }
    }

    private void PlaceItemsInCorrectOrder()
    {
        for (int i = 0; i < items.Length; i++)
        {
            altar.PlaceItemOnPedestal(items[i], i);
        }
        Debug.Log("All items placed!");
    }

    private void ResetAltar()
    {
        for (int i = 0; i < 5; i++)
        {
            altar.RemoveItemFromPedestal(i);
        }
        Debug.Log("Altar reset!");
    }
}
