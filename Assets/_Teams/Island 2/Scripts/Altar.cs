using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Altar : MonoBehaviour
{
    private const int TOTAL_PEDESTALS = 5;
    private const int TOTAL_ITEMS = 5;

    [SerializeField] private Pedestal[] pedestals;
    [SerializeField] private AltarCandle candleSlot;
    [SerializeField] private Transform rewardCompartment;

    private List<AltarItem> availableItems = new List<AltarItem>();
    private bool ritualCompleted;

    // Correct order from heaviest to lightest: Career, Passion, Love, Wealth, Health
    private readonly AltarItem.ItemType[] correctOrder = new AltarItem.ItemType[]
    {
        AltarItem.ItemType.Career,
        AltarItem.ItemType.Passion,
        AltarItem.ItemType.Love,
        AltarItem.ItemType.Wealth,
        AltarItem.ItemType.Health
    };

    private void Start()
    {
        if (pedestals == null || pedestals.Length == 0)
        {
            InitializePedestals();
        }
        if (availableItems.Count == 0)
        {
            CollectAvailableItems();
        }
    }

    private void InitializePedestals()
    {
        if (pedestals == null || pedestals.Length != TOTAL_PEDESTALS)
        {
            pedestals = GetComponentsInChildren<Pedestal>();
        }

        for (int i = 0; i < pedestals.Length; i++)
        {
            pedestals[i].Initialize(i);
        }
    }

    private void CollectAvailableItems()
    {
        availableItems.Clear();
        AltarItem[] allItems = FindObjectsOfType<AltarItem>();
        availableItems.AddRange(allItems.Where(item => item.transform.parent != transform));
    }

    public bool PlaceItemOnPedestal(AltarItem item, int pedestalIndex)
    {
        if (ritualCompleted || pedestalIndex < 0 || pedestalIndex >= pedestals.Length)
        {
            return false;
        }

        if (!pedestals[pedestalIndex].PlaceItem(item))
        {
            return false;
        }

        if (AreAllPedestalsOccupied())
        {
            EnableCandleLighting();
        }

        return true;
    }

    public bool RemoveItemFromPedestal(int pedestalIndex)
    {
        if (ritualCompleted || pedestalIndex < 0 || pedestalIndex >= pedestals.Length)
        {
            return false;
        }

        AltarItem removedItem = pedestals[pedestalIndex].RemoveItem();
        return removedItem != null;
    }

    public bool LightCandle()
    {
        if (ritualCompleted || !AreAllPedestalsOccupied() || candleSlot == null)
        {
            return false;
        }

        candleSlot.Light();

        if (IsOrderCorrect())
        {
            CompleteRitual();
            return true;
        }
        else
        {
            candleSlot.Extinguish();
            return false;
        }
    }

    private void CompleteRitual()
    {
        ritualCompleted = true;
        OpenRewardCompartment();
    }

    private void OpenRewardCompartment()
    {
        if (rewardCompartment != null)
        {
            rewardCompartment.gameObject.SetActive(true);
        }
    }

    private bool AreAllPedestalsOccupied()
    {
        return System.Array.TrueForAll(pedestals, pedestal => pedestal.IsOccupied);
    }

    private bool IsOrderCorrect()
    {
        for (int i = 0; i < pedestals.Length; i++)
        {
            AltarItem item = pedestals[i].PlacedItem;
            if (item == null || item.Type != correctOrder[i])
            {
                return false;
            }
        }
        return true;
    }

    private void EnableCandleLighting()
    {
        if (candleSlot != null)
        {
            candleSlot.gameObject.SetActive(true);
        }
    }

    public bool IsRitualCompleted => ritualCompleted;

    public void SetupReferences(Pedestal[] pedestalArray, AltarCandle candle, Transform reward)
    {
        pedestals = pedestalArray;
        candleSlot = candle;
        rewardCompartment = reward;
    }
}
