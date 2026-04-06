using System;
using UnityEngine;

// Author: Edward
public class PBargainScale : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isLeftArm;
    [SerializeField] private Transform placePoint;

    private BargainItem storedPickup;

    public string InteractMessage
    {
        get
        {
            string side = isLeftArm ? "Left" : "Right";
            return storedPickup == null ? $"Press E to Place Item on the {side} Scale" : "Swap / Take Item";
        }
    }

    public bool ShouldShowMessage(InteractionController interactionController)
    {
        var pickupController = interactionController.GetComponent<PickupController>();
        
        bool playerHasItem = pickupController && pickupController.HasPickup;
        bool slotHasItem = storedPickup;

        return playerHasItem || slotHasItem;
    }

    private void Awake()
    {
        GameEvents.OnItemPlaced += OnPuzzleItemPlaced;
    }

    public void Interact(InteractionController interactionController)
    {
        var pickupController = interactionController.GetComponent<PickupController>();
        if (!pickupController) return;
        
        var playerPickup = pickupController.currentPickup;

        switch (playerPickup)
        {
            // Case 1: Player has nothing, slot has item = Take item
            case null when storedPickup:
                TakeFromSlot(pickupController);
                return;
            // Case 2: Player has item, slot empty = Place item
            case BargainItem when !storedPickup:
                PlaceIntoSlot(pickupController, playerPickup);
                return;
            // Case 3: Both have items = Swap items
            case BargainItem when storedPickup:
                SwapItems(pickupController, playerPickup);
                break;
        }
    }

    private void TakeFromSlot(PickupController pickupController)
    {
        pickupController.GrabPickup(storedPickup);
        storedPickup = null;
    }
    
    private void PlaceIntoSlot(PickupController pickupController, IPickup pickup)
    {
        if (pickupController.TryPlacePickup(pickup, placePoint))
        {
            storedPickup = (BargainItem)pickup;
            GameEvents.ExecOnItemPlaced(storedPickup);
        }
    }

    private void SwapItems(PickupController pickupController, IPickup playerPickup)
    {
        var tempPickup = storedPickup;
        
        // Place player's item into slot
        if (pickupController.TryPlacePickup(playerPickup, placePoint)) storedPickup = (BargainItem)playerPickup;
        
        // Give previous item to player
        pickupController.GrabPickup(tempPickup);
    }

    private void OnPuzzleItemPlaced(IPickup pickup)
    {
        Debug.Log((pickup as BargainItem)?.name + " has been placed.");
    }

    private void OnDestroy()
    {
        GameEvents.OnItemPlaced -= OnPuzzleItemPlaced;
    }
}