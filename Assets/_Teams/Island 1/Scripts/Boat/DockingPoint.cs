using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DockingPoint : MonoBehaviour
{
    [Header("Docking Settings")]
    [Tooltip("The position and rotation the boat will snap to when docked.")]
    [SerializeField] private Transform dockSnapTransform;

    [Tooltip("Where the player appears on land after docking.")]
    [SerializeField] private Transform playerSpawnTransform;

    [Tooltip("Where the boat is teleported when undocking. Place this in the water near the dock.")]
    [SerializeField] private Transform boatSpawnTransform;

    [Header("References")]
    [SerializeField] private DockingPrompt dockingPrompt;

    [Tooltip("The player GameObject (the one with the Movement script and player camera).")]
    [SerializeField] private GameObject player;

    private GameObject boat;
    private Rigidbody boatRigidbody;
    private BoatController boatController;
    private bool boatIsInRange = false;
    private bool isDocked = false;

    private void Update()
    {
        if (isDocked)
        {
            if (IsPlayerAboard())
            {
                dockingPrompt.ShowPrompt("Press F to Undock");
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    UndockBoat();
                }
            }
            else
            {
                dockingPrompt.HidePrompt();
            }
        }
        else
        {
            if (boatIsInRange && IsPlayerAboard())
            {
                dockingPrompt.ShowPrompt("Press F to Dock");
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    DockBoat();
                }
            }
            else
            {
                dockingPrompt.HidePrompt();
            }
        }
    }

    private bool IsPlayerAboard()
    {
        return player != null
               && player.transform.parent != null
               && player.transform.parent.CompareTag("boat");
    }

    private void DockBoat()
    {
        isDocked = true;
        DisableBoatPhysics();
        StartCoroutine(DisembarkSequence());
        dockingPrompt.HidePrompt();
    }

    private IEnumerator DisembarkSequence()
    {
        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider != null) playerCollider.enabled = false;

        boat.transform.position = dockSnapTransform.position;
        boat.transform.rotation = dockSnapTransform.rotation;

        yield return null;

        DisembarkPlayer();

        yield return new WaitForSeconds(0.1f);

        if (playerCollider != null) playerCollider.enabled = true;
    }

    private void DisableBoatPhysics()
    {
        if (boatRigidbody != null)
        {
            boatRigidbody.linearVelocity = Vector3.zero;
            boatRigidbody.angularVelocity = Vector3.zero;
            boatRigidbody.isKinematic = true;
        }

        if (boatController != null)
            boatController.enabled = false;
    }

    private void DisembarkPlayer()
    {
        player.transform.SetParent(null);

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
            playerRb.useGravity = false;
        }

        if (playerSpawnTransform != null)
        {
            player.transform.position = playerSpawnTransform.position;
            player.transform.rotation = playerSpawnTransform.rotation;
        }
        else
        {
            Debug.LogWarning("[DockingPoint] playerSpawnTransform is not assigned!");
        }

        StartCoroutine(EnablePlayerPhysics());
    }

    private IEnumerator EnablePlayerPhysics()
    {
        yield return new WaitForSeconds(0.2f);

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.useGravity = true;
        }
    }

    public void UndockBoat()
    {
        isDocked = false;

        if (boat == null)
        {
            Debug.LogWarning("[DockingPoint] UndockBoat: boat reference is null!");
            return;
        }

        BoardPlayer();

        if (boatSpawnTransform != null)
        {
            boat.transform.position = boatSpawnTransform.position;
            boat.transform.rotation = boatSpawnTransform.rotation;
        }
        else
        {
            Debug.LogWarning("[DockingPoint] boatSpawnTransform is not assigned!");
        }

        if (boatRigidbody != null)
            boatRigidbody.isKinematic = false;

        if (boatController != null)
            boatController.enabled = true;

        dockingPrompt.HidePrompt();
    }

    private void BoardPlayer()
    {
        player.transform.SetParent(boat.transform);

        Transform seatPosition = FindDeepChild(boat.transform, "SeatPosition");
        if (seatPosition != null)
        {
            player.transform.position = seatPosition.position;
            player.transform.rotation = seatPosition.rotation;
        }
        else
        {
            player.transform.localPosition = Vector3.up * 1.5f;
            Debug.LogWarning("[DockingPoint] SeatPosition not found on boat!");
        }

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
            playerRb.useGravity = false;
        }
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("boat"))
        {
            boat = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;

            boatRigidbody = boat.GetComponent<Rigidbody>();
            boatController = boat.GetComponent<BoatController>();
            boatIsInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("boat"))
        {
            boatIsInRange = false;

            if (!isDocked)
            {
                boat = null;
                boatRigidbody = null;
                boatController = null;
            }

            dockingPrompt.HidePrompt();
        }
    }

    private void OnDrawGizmosSelected()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, sc.radius);
        }

        if (dockSnapTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(dockSnapTransform.position, Vector3.one);
            Gizmos.DrawRay(dockSnapTransform.position, dockSnapTransform.forward * 3f);
        }

        if (boatSpawnTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(boatSpawnTransform.position, Vector3.one);
            Gizmos.DrawRay(boatSpawnTransform.position, boatSpawnTransform.forward * 3f);
        }
    }
}