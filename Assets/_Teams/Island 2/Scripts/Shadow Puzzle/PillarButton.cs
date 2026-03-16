using UnityEngine;

public class PillarButton : MonoBehaviour, IInteractable
{
    private PShadowGridManager grid;

    [Header("Grid Settings")]
    [SerializeField] private bool isRowButton;
    [SerializeField] private int direction;     // rows: -1 = left, 1 = right | columns: -1 = down, 1 = up
    [SerializeField] private int index;
    
    [SerializeField] private string objectInteractMessage;
    public string InteractMessage => objectInteractMessage;

    private void Start()
    {
        if (grid == null)
        {
            grid = FindFirstObjectByType<PShadowGridManager>();
        }
    }
    
    public void Interact(InteractionController interactionController)
    {
        if (isRowButton)
        {
            grid.PushRow(index, direction);
        }
        else
        {
            grid.PushColumn(index, direction);
        }
    }
}