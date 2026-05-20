// Created by Jantina

using UnityEngine;
using Assets._Teams.Island_1.Scripts.User_Interface;

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

    private void EnsureReferences()
    {
        if (boat == null)
            boat = GameObject.FindGameObjectWithTag("boat");

        if (playerBoardSpot == null)
        {
            var boardSpotObj = GameObject.FindGameObjectWithTag("seatPosition");
            if (boardSpotObj != null) playerBoardSpot = boardSpotObj.transform;
        }

        if (playerExitSpot == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.CompareTag("dockExit"))
                {
                    playerExitSpot = child;
                    break;
                }
            }
        }

        if (dockUI == null)
        {
            dockUI = FindFirstObjectByType<DockInteractionUI>(FindObjectsInactive.Include); 
            if (dockUI != null)
                Debug.Log("[DockPoint] DockInteractionUI found successfully.");
        }

        if (movement == null)
            movement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include); 

    }

    private void Start()
    {
        EnsureReferences();

        if (boat == null)          Debug.LogWarning($"[DockPoint] 'boat' tag not found yet on {gameObject.name}.", this);
        if (playerBoardSpot == null) Debug.LogWarning($"[DockPoint] 'seatPosition' tag not found yet.", this);
        if (playerExitSpot == null)  Debug.LogWarning($"[DockPoint] 'dockExit' child not found on {gameObject.name}.", this);
        if (movement == null)        Debug.LogWarning($"[DockPoint] No Movement component found yet.", this);
    }

    public void UpdateUI(bool playerIsOnBoat, Vector3 playerPos, Vector3 boatPos)
    {
        EnsureReferences();
        if (dockUI == null) return;

        bool playerNearDock = Vector3.Distance(playerPos, transform.position) <= interactionRange;
        bool boatNearDock   = Vector3.Distance(boatPos,   transform.position) <= boatDockRange;

        if (!playerIsOnBoat && playerNearDock && boatNearDock)
        {
            if (CanBoard())
                dockUI.ShowBoard(true,this);
            else
                dockUI.ShowBlockedReason(GetLockedReason(),this);
        }
        else if (playerIsOnBoat && boatNearDock)
            dockUI.ShowDisembark(this);
        else if (dockUI.IsShowingForDock(this)) // ONLY hide if we're the one currently showing
            dockUI.Hide();
    }

    public void TryInteract(bool playerIsOnBoat, Vector3 playerPos, Vector3 boatPos)
    {
        EnsureReferences();
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
            case RequirementType.None:          return true;
            case RequirementType.PuzzleID:      return PuzzleProgress.IsComplete(puzzleID);
            case RequirementType.StartingItems: return HasStartingItems();
            default:                            return true;
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
                bool hasJournal   = movement.GetComponentInChildren<Journal>()   != null;
                bool hasAstrolabe = movement.GetComponentInChildren<Astrolabe>() != null;
                if (!hasJournal && !hasAstrolabe) return "You need the Journal and Astrolabe.";
                if (!hasJournal)                  return "You need the Journal.";
                if (!hasAstrolabe)                return "You need the Astrolabe.";
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