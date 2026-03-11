using UnityEngine;

// Author: Edward
public interface IPickup : IInteractable
{
    public void Grab(PickupController pickupController);
    public void Drop(PickupController pickupController);
    public void SetPositionInParent(Transform newParent);
    public void Use();
}