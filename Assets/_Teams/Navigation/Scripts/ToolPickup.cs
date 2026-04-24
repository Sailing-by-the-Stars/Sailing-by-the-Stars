using UnityEngine;

public class ToolPickup : MonoBehaviour, IPickup
{
    [Header("Attachment Settings")]
    [SerializeField] private Vector3 pickupPositionOffset;
    
    public virtual string InteractMessage => "Press E to Pickup";
    
    public void Interact(InteractionController interactionController)
    {
        var pickupController = interactionController.GetComponent<PickupController>();
        Grab(pickupController);
    }

    public virtual void Grab(PickupController pickupController)
    { 
        transform.parent = pickupController.transform.Find("Main Camera").transform;
        transform.localPosition = pickupPositionOffset;
        transform.localRotation = Quaternion.identity;
        GameEvents.ExecOnPickup(this);
    }

    public void Drop(PickupController pickupController)
    {
    }

    public void SetPositionInParent(Transform newParent)
    {
    }

    public void Use()
    {
    }
}
