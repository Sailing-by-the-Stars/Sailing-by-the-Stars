using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/** Interaction System Documentation - How to Use:
 *
 * If you need an object to be interactable,
 * simply create a new script that implements the IInteractable interface
 * and put any custom logic that needs to happen on interaction in
 * the overriden Interact() function.
 */

// Author: Edward
public class InteractionController : MonoBehaviour
{
    [Header("Interaction References & Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private float interactionDistance = 5f;

    private IInteractable currentTargetedInteractable;

    private void Update()
    {
        UpdateCurrentInteractable();
        UpdateInteractionText();
        CheckForInteractionInput();
    }
    
    private void UpdateCurrentInteractable()
    {
        var ray = playerCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        
        Physics.Raycast(ray, out RaycastHit hit, interactionDistance, ~0, QueryTriggerInteraction.Ignore);
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);
        
        currentTargetedInteractable = hit.collider?.GetComponent<IInteractable>();
    }

    private void UpdateInteractionText()
    {
        if (currentTargetedInteractable == null)
        {
            interactionText.text = string.Empty;
            return;
        }

        interactionText.text = currentTargetedInteractable.InteractMessage;
    }

    private void CheckForInteractionInput()
    {
        // TODO: replace hardcoded key press with Input Actions
        if (Keyboard.current.eKey.wasPressedThisFrame && currentTargetedInteractable != null)
        {
            currentTargetedInteractable.Interact(this);
        }
    }
}