using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Altar : MonoBehaviour, IInteractable
{
    [System.Serializable]
    private class Pedestal
    {
        public int index;
        public Transform pedestalRoot;
        public Transform placePoint;
        
        [HideInInspector] public BargainItem storedPickup;
    }
    
    public string InteractMessage => "Press E to Place / Swap Item";

    [SerializeField] private List<Pedestal> pedestals = new();
    [SerializeField] private Light[] candles;
    [SerializeField] private Transform rewardDoor;
    private bool ritualCompleted;
    
    [SerializeField] private float rewardDoorTargetOffset = -.03f;
    [SerializeField] private float rewardDoorMoveSpeed = 5f;
    private Vector3 rewardDoorStartPos;
    private float rewardDoorCurrentOffset;

    private void Awake()
    {
        GameEvents.OnAltarItemPlaced += IsOrderCorrect;
        
        for (int i = 0; i < pedestals.Count; i++)
        {
            pedestals[i].index = i + 1;
        }
        
        rewardDoorStartPos = rewardDoor.localPosition;
    }

    private void Update()
    {
        OpenRewardCompartment();
    }

    public void Interact(InteractionController interactionController)
    {
        var pedestal = GetPedestalFromHit(interactionController);
        if (pedestal == null) return;
        
        var pickupController = interactionController.GetComponent<PickupController>();
        if (!pickupController) return;
        
        var playerPickup = pickupController.currentPickup as BargainItem;
        
        // Case 1: Player has nothing, slot has item = Take item
        if (!playerPickup && pedestal.storedPickup)
        {
            TakeFromPedestal(pedestal, pickupController);
            return;
        }
        
        // Case 2: Player has item, slot empty = Place item
        if (playerPickup && !pedestal.storedPickup)
        {
            PlaceIntoPedestal(pedestal, pickupController, playerPickup);
            return;
        }
        
        // Case 3: Both have items = Swap items
        if (playerPickup && pedestal.storedPickup)
        {
            SwapItems(pedestal, pickupController, playerPickup);
        }
    }
    
    private Pedestal GetPedestalFromHit(InteractionController interactionController)
    {
        var hitTransform = interactionController.CurrentHitTransform;
        return !hitTransform ? null : pedestals.FirstOrDefault(pedestal => hitTransform == pedestal.pedestalRoot || hitTransform.IsChildOf(pedestal.pedestalRoot));
    }
    
    private void TakeFromPedestal(Pedestal pedestal, PickupController pickupController)
    {
        var item = pedestal.storedPickup;
        pedestal.storedPickup = null;

        item.Grab(pickupController);
    }
    
    private void PlaceIntoPedestal(Pedestal pedestal, PickupController pickupController, BargainItem pickup)
    {
        pickupController.TryPlacePickup(pickup, pedestal.placePoint);
        pedestal.storedPickup = pickup;
        
        GameEvents.ExecOnAltarItemPlaced();
    }

    private void SwapItems(Pedestal pedestal, PickupController pickupController, BargainItem playerPickup)
    {
        var oldPickup = pedestal.storedPickup;

        pickupController.TryPlacePickup(playerPickup, pedestal.placePoint);
        pedestal.storedPickup = playerPickup;
        
        oldPickup.Grab(pickupController);
    }

    private void CompleteRitual()
    {
        if (ritualCompleted || candles.Length == 0) return;

        foreach (var candle in candles)
            candle.enabled = true;
        ritualCompleted = true;
        Debug.Log("Bargain Completed.");
    }

    private void OpenRewardCompartment()
    {
        if (!rewardDoor || !ritualCompleted) return;
        
        rewardDoorCurrentOffset = Mathf.Lerp(rewardDoorCurrentOffset, rewardDoorTargetOffset, rewardDoorMoveSpeed * Time.deltaTime);
        rewardDoor.localPosition = rewardDoorStartPos + Vector3.forward * rewardDoorCurrentOffset;
    }

    private void IsOrderCorrect()
    {
        Debug.Log("Item placed on pedestal");
        if (pedestals.All(pedestal => pedestal.storedPickup && pedestal.storedPickup.weight == pedestal.index))
            CompleteRitual();
    }

    private void OnDestroy()
    {
        GameEvents.OnAltarItemPlaced -= IsOrderCorrect;
    }
}
