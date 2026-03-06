using UnityEngine;
using UnityEngine.InputSystem;

/** Pickup Documentation - How to Use:
 *
 * Make sure to assign an empty GameObject to the Player as the anchor where picked up objects will appear (pickupHolder).
 * Assuming that most pick up objects will be physics objects (Rigidbody + Collider), then
 * simply make a new script derived from PhysicsPickup and
 * override Interact(), Grab(), Drop(), Use() as needed per object.
 * See TorchPickup.cs for example.
 *
 * If a non-physics object needs to be picked up, then create a new base class that implements the IPickup interface,
 * from which new per-object scripts can be derived. Use PhysicsPickup.cs as a reference.
 */

public class PickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private Transform pickupHolder;
    
    private IPickup currentPickup;
    
    public bool HasPickup => currentPickup != null;

    public void GrabPickup(IPickup newPickup)
    {
        currentPickup = newPickup;
        
        currentPickup.SetPositionInParent(pickupHolder);
    }

    private void Update()
    {
        CheckDropInput();
        CheckPickupInput();
    }

    private void CheckDropInput()
    {
        // TODO: replace hardcoded key press with Input Actions
        if (!Keyboard.current.qKey.wasPressedThisFrame || !HasPickup) return;
        
        currentPickup.Drop(this);
        currentPickup = null;
    }

    private void CheckPickupInput()
    {
        // TODO: replace hardcoded key press with Input Actions
        if (!Mouse.current.leftButton.wasPressedThisFrame || !HasPickup) return;

        currentPickup.Use();
    }
}