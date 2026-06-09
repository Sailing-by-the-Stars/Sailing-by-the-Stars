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

    private bool pedestalsFull = false;
    public List<ConditionalDialogue> dialogues;
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
    }

    private void OpenRewardCompartment()
    {
        if (!rewardDoor || !ritualCompleted) return;
        
        Debug.Log("Bargain Completed.");
        PuzzleProgress.MarkComplete("bargaining");
        
        rewardDoorCurrentOffset = Mathf.Lerp(rewardDoorCurrentOffset, rewardDoorTargetOffset, rewardDoorMoveSpeed * Time.deltaTime);
        rewardDoor.localPosition = rewardDoorStartPos + Vector3.forward * rewardDoorCurrentOffset;
    }

    private void IsOrderCorrect()
    {
        pedestalsFull = pedestals.All(pedestal => pedestal.storedPickup);

        Debug.Log("Item placed on pedestal");
        if (pedestalsFull && pedestals.All(pedestal => pedestal.storedPickup.weight == pedestal.index))
            CompleteRitual();
        else if (pedestalsFull)
            StartDialogue();
    }

    private void OnDestroy()
    {
        GameEvents.OnAltarItemPlaced -= IsOrderCorrect;
    }

    private void StartDialogue()
    {
        for (int i = dialogues.Count - 1; i >= 0; i--)
        {
            var entry = dialogues[i];

            bool valid = true;

            if (entry.conditions != null && entry.conditions.Count > 0)
            {
                foreach (var cond in entry.conditions)
                {
                    if (!cond.Evaluate(PlayerState.Instance))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (valid)
            {
                DialogueSystem.Instance.StartDialogue(entry.dialogue, gameObject);
                return;
            }
        }

        Debug.LogWarning("No valid idle dialogue found for this area.");
    }
}
