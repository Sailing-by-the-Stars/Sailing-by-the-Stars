using UnityEngine;

public class RewardItem : PhysicsPickup
{
    public override void Grab(PickupController pickupController)
    {
        Collect();
        base.CollectAndDestroy();
    }
    
    [Header("Quest Objective ID")]
    [SerializeField] private int itemID;
    [SerializeField] private string puzzleProgressID;

    private void Collect()
    {
        QuestManager.Instance.RegisterItemCollected(itemID);
        PuzzleProgress.MarkComplete(puzzleProgressID);
    }
}
