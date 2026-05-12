/*
 *  DockPoint.cs
 *
 *  SETUP PER DOCK:
 *    1. Add this component to each dock GameObject.
 *    2. Set requirementType and puzzleID (if needed) in the Inspector.
 *    3. Tag the boat's seat empty GameObject as "seatPosition".
 *    4. Tag the exit child of this prefab as "dockExit".
 *    5. Everything else is found automatically on Start.
 *
 *  Tags needed in project:
 *    "boat"         - already exists
 *    "seatPosition" - empty GO on the boat where the player sits
 *    "dockExit"     - child of this prefab where the player lands on disembark
 */

using UnityEngine;

public class DockPoint : MonoBehaviour
{
    public enum RequirementType { None, PuzzleID, StartingItems }

    [Header("Requirement")]
    [SerializeField] private RequirementType requirementType = RequirementType.None;
    [SerializeField] private string puzzleID = "";

    [Header("Detection")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float boatDockRange = 8f;

    private GameObject boat;
    private Transform playerBoardSpot;
    private Transform playerExitSpot;
    private DockInteractionUI dockUI;
    private Movement movement;

    private void Start()
    {
        boat = GameObject.FindGameObjectWithTag("boat");
        if (boat == null)
            Debug.LogError($"[DockPoint] No GameObject tagged 'boat' found.", this);

        GameObject boardSpotObj = GameObject.FindGameObjectWithTag("seatPosition");
        if (boardSpotObj != null)
            playerBoardSpot = boardSpotObj.transform;
        else
            Debug.LogError($"[DockPoint] No GameObject tagged 'seatPosition' found.", this);

        GameObject exitObj = null;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("dockExit"))
            {
                exitObj = child.gameObject;
                break;
            }
        }
        if (exitObj != null)
            playerExitSpot = exitObj.transform;
        else
            Debug.LogError($"[DockPoint] No child tagged 'dockExit' found on {gameObject.name}.", this);

        dockUI = FindFirstObjectByType<DockInteractionUI>();

        movement = FindFirstObjectByType<Movement>();
        if (movement == null)
            Debug.LogError($"[DockPoint] No Movement component found in scene.", this);
    }

    public void UpdateUI(bool playerIsOnBoat, Vector3 playerPos, Vector3 boatPos)
    {
        if (dockUI == null) return;

        bool playerNearDock = Vector3.Distance(playerPos, transform.position) <= interactionRange;
        bool boatNearDock   = Vector3.Distance(boatPos,   transform.position) <= boatDockRange;

        if (!playerIsOnBoat && playerNearDock && boatNearDock)
            dockUI.ShowBoard(CanBoard());
        else if (playerIsOnBoat && boatNearDock)
            dockUI.ShowDisembark();
        else
            dockUI.Hide();
    }

    public void TryInteract(bool playerIsOnBoat, Vector3 playerPos, Vector3 boatPos)
    {
        if (boat == null || movement == null) return;

        bool playerNearDock = Vector3.Distance(playerPos, transform.position) <= interactionRange;
        bool boatNearDock   = Vector3.Distance(boatPos,   transform.position) <= boatDockRange;

        if (!boatNearDock) return;

        if (!playerIsOnBoat && playerNearDock)
            TryBoard();
        else if (playerIsOnBoat)
            TryDisembark();
    }

    private void TryBoard()
    {
        if (!CanBoard())
        {
            Debug.Log($"[DockPoint] Boarding blocked at {gameObject.name}: {GetLockedReason()}");
            return;
        }

        movement.BoardBoat(playerBoardSpot);
    }

    private void TryDisembark()
    {
        movement.DisembarkBoat(playerExitSpot);
    }

    private bool CanBoard()
    {
        switch (requirementType)
        {
            case RequirementType.None:           return true;
            case RequirementType.PuzzleID:       return PuzzleProgress.IsComplete(puzzleID);
            case RequirementType.StartingItems:  return HasStartingItems();
            default:                             return true;
        }
    }

    private bool HasStartingItems()
    {
        if (movement == null) return false;
        bool hasJournal   = movement.GetComponentInChildren<Journal>()   != null;
        bool hasAstrolabe = movement.GetComponentInChildren<Astrolabe>() != null;
        return hasJournal && hasAstrolabe;
    }

    private string GetLockedReason()
    {
        switch (requirementType)
        {
            case RequirementType.PuzzleID:
                return $"Complete the {puzzleID} puzzle first.";
            case RequirementType.StartingItems:
                if (movement == null) return "Requirements not met.";
                bool hasJ = movement.GetComponentInChildren<Journal>()   != null;
                bool hasA = movement.GetComponentInChildren<Astrolabe>() != null;
                if (!hasJ && !hasA) return "You need the Journal and Astrolabe.";
                if (!hasJ)          return "You need the Journal.";
                if (!hasA)          return "You need the Astrolabe.";
                break;
        }
        return "Requirements not met.";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, boatDockRange);
    }
}