using UnityEngine;

public class RewardItem : PhysicsPickup
{
    public override void Grab(PickupController pickupController)
    {
        base.Grab(pickupController);
        Collect();
    }
    
    [Header("Quest Objective ID")]
    [SerializeField] private int itemID;

    private void Collect()
    {
        QuestManager.Instance.RegisterItemCollected(itemID);
    }
}
