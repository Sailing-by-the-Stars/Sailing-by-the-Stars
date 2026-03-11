using UnityEngine;

// Author: Edward
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PhysicsPickup : MonoBehaviour, IPickup
{
    [Header("Attachment Settings")]
    [SerializeField] private Vector3 pickupPositionOffset;
    
    private Rigidbody pickupRigidbody;
    private Collider pickupCollider;

    public virtual string InteractMessage => "Press E to Pickup";

    protected virtual void Awake()
    {
        pickupRigidbody = GetComponent<Rigidbody>();
        pickupCollider = GetComponent<Collider>();
    }
    
    public void Interact(InteractionController interactionController)
    {
        var pickupController = interactionController.GetComponent<PickupController>();
        Grab(pickupController);
    }

    public virtual void Grab(PickupController pickupController)
    {
        if (!pickupController || pickupController.HasPickup) return;

        pickupController.GrabPickup(this);

        SetPhysicsValue(true);
    }

    public virtual void Drop(PickupController pickupController)
    {
        transform.parent = null;
        
        SetPhysicsValue(false);
    }

    public void SetPositionInParent(Transform newParent)
    {
        transform.parent = newParent;
        transform.localPosition = pickupPositionOffset;
        transform.localRotation = Quaternion.identity;
    }

    public virtual void Use()
    {
        Debug.Log("Pickup Used!");
    }

    private void SetPhysicsValue(bool wasPickedUp)
    {
        pickupRigidbody.isKinematic = wasPickedUp;
        pickupCollider.enabled = !wasPickedUp;
    }
}