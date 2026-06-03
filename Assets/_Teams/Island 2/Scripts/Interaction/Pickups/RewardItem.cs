using System;
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
    [SerializeField] private int pageID = 0;
    [SerializeField] private string puzzleProgressID;

    public static Action<int> collectedReward;

    private void Collect()
    {
        QuestManager.Instance.RegisterItemCollected(itemID);
        PuzzleProgress.MarkComplete(puzzleProgressID);
        collectedReward?.Invoke(pageID);
    }
}
