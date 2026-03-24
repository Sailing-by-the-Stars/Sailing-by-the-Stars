using UnityEngine;
using UnityEngine.Events;

// Author: Edward
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class TempPhysicsPickup : MonoBehaviour, IPickup
{
    [Header("Attachment Settings")]
    [SerializeField] private Vector3 pickupPositionOffset;
    
    private Rigidbody pickupRigidbody;
    private Collider pickupCollider;

    public UnityEvent OnGrab = new();
    public UnityEvent OnUse = new();
    public UnityEvent OnDrop = new();

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
        OnGrab.Invoke();
    }

    public virtual void Drop(PickupController pickupController)
    {
        transform.parent = null;
        
        SetPhysicsValue(false);
        OnDrop.Invoke();
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
        OnUse.Invoke();
    }

    private void SetPhysicsValue(bool wasPickedUp)
    {
        pickupRigidbody.isKinematic = wasPickedUp;
        pickupCollider.enabled = !wasPickedUp;
    }
}