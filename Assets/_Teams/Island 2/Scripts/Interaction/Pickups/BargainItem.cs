using UnityEngine;

// Author: Edward
public class BargainItem : PhysicsPickup
{   
    [Header("Bargain Puzzle")]
    [Tooltip("Determines which item is more 'important'")]
    public int weight;
}