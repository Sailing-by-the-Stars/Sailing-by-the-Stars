using UnityEngine;
using UnityEngine.InputSystem;

public class PickupController : MonoBehaviour
{
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
        if (!Keyboard.current.qKey.wasPressedThisFrame || !HasPickup) return;
        
        currentPickup.Drop(this);
        currentPickup = null;
    }

    private void CheckPickupInput()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame || !HasPickup) return;

        currentPickup.Use();
    }
}