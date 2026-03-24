using UnityEngine;

public class PShadowPillarButton : MonoBehaviour, IInteractable
{
    private PShadowGridManager gridManager;

    [Header("Grid Settings")]
    [SerializeField] private bool isRowButton;
    [Tooltip("Rows: -1 = left, 1 = right | Columns: -1 = down, 1 = up")]
    [SerializeField] private int direction;
    [Tooltip("Which row/column this button affects.")]
    [SerializeField] private int index;
    
    [Tooltip("If this is true, all other settings are irrelevant!")]
    [SerializeField] private bool isResetButton = false;
    
    [SerializeField] private string objectInteractMessage = "Press E to Push Pillar(s)";
    public string InteractMessage => objectInteractMessage;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<PShadowGridManager>();
        }
    }
    
    public void Interact(InteractionController interactionController)
    {
        if (isResetButton)
        {
            gridManager.ResetPillars();
            return;
        }
        
        if (isRowButton)
        {
            gridManager.PushRow(index, direction);
        }
        else
        {
            gridManager.PushColumn(index, direction);
        }
    }
}