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

    private RaycastHit currentHit;
    private IInteractable activeInteractable;
    private IInteractable currentTargetedInteractable;
    public Transform CurrentHitTransform => currentHit.collider ? currentHit.collider.transform : null;

    PlayerControls playerControls;

    private void Start()
    {
        playerControls = TempStateMachine.Instance.PlayerControls;
    }

    private void Update()
    {
        UpdateCurrentInteractable();
        UpdateInteractionText();
        CheckForInteractionInput();
    }
    
    private void UpdateCurrentInteractable()
    {
        var ray = playerCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        
        Physics.Raycast(ray, out currentHit, interactionDistance, ~0, QueryTriggerInteraction.Ignore);
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);
        
        var interactableTargeted = currentHit.collider?.GetComponentInParent<IInteractable>();
        currentTargetedInteractable = interactableTargeted != null &&
                                      interactableTargeted.ShouldShowMessage(this)
                                      ? interactableTargeted : null;
    }

    private void UpdateInteractionText()
    {
        if (currentTargetedInteractable == null || DialogueSystem.Instance.isDialogueActive)
        {
            interactionText.text = string.Empty;
            return;
        }

        interactionText.text = currentTargetedInteractable.InteractMessage;
    }

    private void CheckForInteractionInput()
    {
        if (currentTargetedInteractable == null || DialogueSystem.Instance.isDialogueActive) return;

        // TODO: replace hardcoded key press with Input Actions
        var key = playerControls.Interaction.Pickup;

        if (key.WasPerformedThisFrame())
        {
            activeInteractable = currentTargetedInteractable;

            activeInteractable.Interact(this);
            activeInteractable.HoldInteract(this);
        }

        if (key.WasReleasedThisFrame() && activeInteractable != null)
        {
            activeInteractable.ReleaseInteract(this);
            activeInteractable = null;
        }
    }
}