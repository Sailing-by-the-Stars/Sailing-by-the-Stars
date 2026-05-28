using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Image interactionBackground;
    [SerializeField] private float interactionDistance = 5f;

    private RaycastHit currentHit;
    private IInteractable activeInteractable;
    private IInteractable currentTargetedInteractable;
    public Transform CurrentHitTransform => currentHit.collider ? currentHit.collider.transform : null;

    private PlayerControls playerControls;

    private void Start()
    {
        playerControls = TempStateMachine.Instance.PlayerControls;
        interactionBackground = interactionText.GetComponentInParent<Image>();
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
            interactionBackground.enabled = false;
            interactionText.text = string.Empty;
            return;
        }

        interactionBackground.enabled = true;
        // Note: For some reason you can't save tags (<color></color>) in interfaces, they just get dropped,
        // so it's necessary to add the tags back here. Yay, Unity!
        string richText = Regex.Replace(currentTargetedInteractable.InteractMessage, @"\bE\b", "<color=#F0E37D>E</color>");
        interactionText.text = richText;
    }

    private void CheckForInteractionInput()
    {
        if (currentTargetedInteractable == null || DialogueSystem.Instance.isDialogueActive) return;
        
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