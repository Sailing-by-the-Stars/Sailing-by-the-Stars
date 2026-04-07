using UnityEngine;

// Author: Edward
public class PBargainScale : MonoBehaviour, IInteractable
{
    [System.Serializable]
    private class Arm
    {
        public Transform armRoot;
        public Transform placePoint;
        [HideInInspector] public BargainItem storedPickup;
    }
    
    [Header("Scale Arms")]
    [SerializeField] private Arm leftArm;
    [SerializeField] private Arm rightArm;

    [SerializeField] private float maxOffset = 0.2f;
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;
    private float currentOffset;
    private float targetOffset;

    public string InteractMessage => "Press E to Place / Swap Item";

    public bool ShouldShowMessage(InteractionController interactionController)
    {
        var arm = GetArmFromHit(interactionController);
        if (arm == null) return false;
        
        var pickupController = interactionController.GetComponent<PickupController>();
        if (!pickupController) return false;

        bool armHasItem = arm.storedPickup;
        bool playerHasBargainItem = pickupController.currentPickup is BargainItem;
        
        if (armHasItem) return !pickupController.HasPickup || playerHasBargainItem;
        
        return playerHasBargainItem;
    }

    private void Awake()
    {
        GameEvents.OnItemPlaced += OnPuzzleItemPlaced;
        
        leftStartPos = leftArm.armRoot.localPosition;
        rightStartPos = rightArm.armRoot.localPosition;
    }

    private void Update()
    {
        AnimateScale();
    }

    public void Interact(InteractionController interactionController)
    {
        var arm = GetArmFromHit(interactionController);
        if (arm == null) return;
        
        var pickupController = interactionController.GetComponent<PickupController>();
        if (!pickupController) return;
        
        var playerPickup = pickupController.currentPickup as BargainItem;
        
        // Case 1: Player has nothing, slot has item = Take item
        if (!playerPickup && arm.storedPickup)
        {
            TakeFromArm(arm, pickupController);
            UpdateTargetOffset();
            return;
        }
        
        // Case 2: Player has item, slot empty = Place item
        if (playerPickup && !arm.storedPickup)
        {
            PlaceIntoArm(arm, pickupController, playerPickup);
            UpdateTargetOffset();
            return;
        }
        
        // Case 3: Both have items = Swap items
        if (playerPickup && arm.storedPickup)
        {
            SwapItems(arm, pickupController, playerPickup);
            UpdateTargetOffset();
        }
    }

    private Arm GetArmFromHit(InteractionController interactionController)
    {
        var hitTransform = interactionController.CurrentHitTransform;
        if (!hitTransform) return null;

        if (IsUnder(hitTransform, leftArm.armRoot)) return leftArm;
        return IsUnder(hitTransform, rightArm.armRoot) ? rightArm : null;
    }

    private static bool IsUnder(Transform child, Transform root)
    {
        return child && root && (child == root || child.IsChildOf(root));
    }

    private void TakeFromArm(Arm arm, PickupController pickupController)
    {
        var item = arm.storedPickup;
        arm.storedPickup = null;

        item.Grab(pickupController);
    }
    
    private void PlaceIntoArm(Arm arm, PickupController pickupController, BargainItem pickup)
    {
        pickupController.TryPlacePickup(pickup, arm.placePoint);
        arm.storedPickup = pickup;
        
        GameEvents.ExecOnItemPlaced(pickup);
    }

    private void SwapItems(Arm arm, PickupController pickupController, BargainItem playerPickup)
    {
        var oldPickup = arm.storedPickup;

        pickupController.TryPlacePickup(playerPickup, arm.placePoint);
        arm.storedPickup = playerPickup;
        
        GameEvents.ExecOnItemPlaced(playerPickup);
        
        oldPickup.Grab(pickupController);
    }

    private void AnimateScale()
    {
        currentOffset = Mathf.Lerp(currentOffset, targetOffset, Time.deltaTime * moveSpeed);

        leftArm.armRoot.localPosition = leftStartPos + Vector3.up * currentOffset;
        rightArm.armRoot.localPosition = rightStartPos - Vector3.up * currentOffset;
    }

    private void UpdateTargetOffset()
    {
        int leftWeight = GetItemWeight(leftArm.storedPickup);
        int rightWeight = GetItemWeight(rightArm.storedPickup);
        
        int difference = rightWeight - leftWeight;
        float normalized = Mathf.Clamp(difference, -1f, 1f);

        targetOffset = normalized * maxOffset;
    }

    private int GetItemWeight(BargainItem item)
    {
        return item != null ? item.weight : 0;
    }

    private void OnPuzzleItemPlaced(IPickup pickup)
    {
        Debug.Log((pickup as BargainItem)?.name + " has a weight of: " + (pickup as BargainItem)?.weight);
    }

    private void OnDestroy()
    {
        GameEvents.OnItemPlaced -= OnPuzzleItemPlaced;
    }
}