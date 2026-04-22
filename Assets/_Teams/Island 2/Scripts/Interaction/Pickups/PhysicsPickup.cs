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
        
        GameEvents.ExecOnPickup(this);
    }

    public virtual void Drop(PickupController pickupController)
    {
        transform.parent = null;
        SetPhysicsValue(false);
        pickupRigidbody.useGravity = true;
        
        GameEvents.ExecOnDrop(this);
    }

    public void SetPositionInParent(Transform newParent)
    {
        Vector3 worldScale = transform.lossyScale;
        
        transform.parent = newParent;
        transform.localPosition = pickupPositionOffset;
        transform.localRotation = Quaternion.identity;
        
        // All this fancy stuff just because Unity by default changes the scale of an object to match it to its new parent...
        Vector3 parentScale = newParent.lossyScale;
        transform.localScale = new Vector3(
            worldScale.x / parentScale.x,
            worldScale.y / parentScale.y,
            worldScale.z / parentScale.z
        );
    }

    public virtual void Use()
    {
        Debug.Log("Pickup Used!");
        
        GameEvents.ExecOnUse(this);
    }

    private void SetPhysicsValue(bool wasPickedUp)
    {
        pickupRigidbody.isKinematic = wasPickedUp;
        pickupCollider.enabled = !wasPickedUp;
    }
}