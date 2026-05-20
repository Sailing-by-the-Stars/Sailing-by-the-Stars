using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place on the PedestalSystem GameObject.
/// </summary>
public class AcceptanceIslandManager : MonoBehaviour
{
    [Header("Pedestals — drag in order, bottom to top")]
    public List<ItemPedestal> pedestals = new List<ItemPedestal>();

    [Header("Door")]
    public GameObject doorObject;
    public string creditsChildName = "Credits";
    public string doorGlowChildName = "DoorGlow";

    [Header("Optional feedback")]
    public AudioSource doorSound;

    private int _filledCount;
    private bool _doorUnlocked;
    private GameObject _creditsChild;
    private GameObject _doorGlowChild;

    private void Awake()
    {
        // Use Awake instead of Start so references are ready before
        // any pedestal could possibly call OnPedestalFilled.
        Debug.Log($"[AcceptanceIslandManager] Awake — {pedestals.Count} pedestals assigned.");

        for (int i = 0; i < pedestals.Count; i++)
        {
            if (pedestals[i] == null)
                Debug.LogWarning($"[AcceptanceIslandManager] Pedestal slot {i} is null!");
            else
            {
                pedestals[i].pedestalManager = this;
                Debug.Log($"[AcceptanceIslandManager] Injected into pedestal {i}: '{pedestals[i].name}'");
            }
        }

        if (doorObject == null)
        {
            Debug.LogWarning("[AcceptanceIslandManager] Door Object is not assigned!");
            return;
        }

        LogChildNames(doorObject.transform);

        _creditsChild  = FindChildByName(doorObject.transform, creditsChildName);
        _doorGlowChild = FindChildByName(doorObject.transform, doorGlowChildName);

        if (_creditsChild  != null) { _creditsChild.SetActive(false);  Debug.Log($"[AcceptanceIslandManager] '{creditsChildName}' found and disabled."); }
        else Debug.LogWarning($"[AcceptanceIslandManager] Child '{creditsChildName}' NOT found on '{doorObject.name}'.");

        if (_doorGlowChild != null) { _doorGlowChild.SetActive(false); Debug.Log($"[AcceptanceIslandManager] '{doorGlowChildName}' found and disabled."); }
        else Debug.LogWarning($"[AcceptanceIslandManager] Child '{doorGlowChildName}' NOT found on '{doorObject.name}'.");
    }

    /// <summary>
    /// Returns true only when every pedestal's puzzle has been completed.
    /// Used by ItemPedestal to gate interaction until all 3 puzzles are done.
    /// </summary>
    public bool AreAllPuzzlesComplete()
    {
        foreach (ItemPedestal p in pedestals)
        {
            if (p == null) continue;
            if (!PuzzleProgress.IsComplete(p.pageID)) return false;
        }
        return true;
    }

    public void OnPedestalFilled(ItemPedestal pedestal)
    {
        _filledCount++;
        Debug.Log($"[AcceptanceIslandManager] OnPedestalFilled — '{pedestal.name}' — {_filledCount}/{pedestals.Count}");

        if (_filledCount >= pedestals.Count)
            UnlockDoor();
    }

    private void UnlockDoor()
    {
        if (_doorUnlocked) return;
        _doorUnlocked = true;

        Debug.Log("[AcceptanceIslandManager] All pedestals filled — unlocking door!");

        if (_creditsChild  != null) { _creditsChild.SetActive(true);  Debug.Log($"[AcceptanceIslandManager] '{creditsChildName}' activated."); }
        else Debug.LogWarning("[AcceptanceIslandManager] Credits child is null — cannot activate.");

        if (_doorGlowChild != null) { _doorGlowChild.SetActive(true); Debug.Log($"[AcceptanceIslandManager] '{doorGlowChildName}' activated."); }
        else Debug.LogWarning("[AcceptanceIslandManager] Door glow child is null — cannot activate.");

        if (doorSound != null) doorSound.Play();
    }

    private void LogChildNames(Transform parent, string indent = "")
    {
        foreach (Transform child in parent)
        {
            Debug.Log($"[AcceptanceIslandManager] Door child: {indent}'{child.name}'");
            LogChildNames(child, indent + "  ");
        }
    }

    private GameObject FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child.gameObject;
            GameObject found = FindChildByName(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (ItemPedestal p in pedestals)
        {
            if (p == null) continue;
            Gizmos.color = p.IsFilled ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, p.transform.position);
        }
    }
}