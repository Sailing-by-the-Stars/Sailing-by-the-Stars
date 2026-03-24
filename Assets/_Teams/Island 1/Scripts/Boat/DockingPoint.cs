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
    [SerializeField] private GameObject player;

    [Header("Undock Prompt Settings")]
    [Tooltip("How close the player must be to the docked boat to see the undock prompt.")]
    [SerializeField] private float undockPromptRadius = 5f;

    private GameObject boat;
    private Rigidbody boatRigidbody;
    private BoatController boatController;
    private BuoyancyController buoyancyController;
    private Collider boatCollider;
    private bool boatIsInRange = false;
    private bool isDocked = false;
    private bool isUndocking = false;

    private int physicsRestoreToken = 0;

    private void Update()
    {
        if (isDocked)
        {
            if (IsPlayerNearDockedBoat())
            {
                dockingPrompt.ShowPrompt("Press F to Undock");
                if (Keyboard.current.fKey.wasPressedThisFrame)
                    UndockBoat();
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
                    DockBoat();
            }
            else
            {
                dockingPrompt.HidePrompt();
            }
        }
    }

    private bool IsPlayerNearDockedBoat()
    {
        if (player == null || dockSnapTransform == null) return false;
        return Vector3.Distance(player.transform.position, dockSnapTransform.position) <= undockPromptRadius;
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

        // Disable the boat's collider so Movement.OnTriggerEnter can't
        // fire and let the player board while the boat is docked.
        if (boatCollider != null)
            boatCollider.enabled = false;

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
            boatRigidbody.isKinematic = true;
            boatRigidbody.linearVelocity = Vector3.zero;
            boatRigidbody.angularVelocity = Vector3.zero;
        }

        if (boatController != null)
            boatController.enabled = false;

        if (buoyancyController != null)
            buoyancyController.enabled = false;
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

        physicsRestoreToken++;
        StartCoroutine(EnablePlayerPhysics(physicsRestoreToken));
    }

    private IEnumerator EnablePlayerPhysics(int token)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
            playerRb.useGravity = false;
        }

        yield return new WaitForSeconds(0.2f);

        if (token != physicsRestoreToken) yield break;

        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.useGravity = true;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
    }

    public void UndockBoat()
    {
        isUndocking = true;
        isDocked = false;

        physicsRestoreToken++;

        if (boat == null)
        {
            Debug.LogWarning("[DockingPoint] UndockBoat: boat reference is null!");
            isUndocking = false;
            return;
        }

        if (boatSpawnTransform != null)
        {
            boat.transform.position = boatSpawnTransform.position;
            boat.transform.rotation = boatSpawnTransform.rotation;
        }
        else
        {
            Debug.LogWarning("[DockingPoint] boatSpawnTransform is not assigned!");
        }

        if (buoyancyController != null)
            buoyancyController.enabled = true;

        if (boatRigidbody != null)
        {
            boatRigidbody.isKinematic = false;
            boatRigidbody.linearVelocity = Vector3.zero;
            boatRigidbody.angularVelocity = Vector3.zero;
        }

        if (boatController != null)
            boatController.enabled = true;

        // Re-enable the collider now the boat is back in the water
        // so the player can board normally again.
        if (boatCollider != null)
            boatCollider.enabled = true;

        dockingPrompt.HidePrompt();

        StartCoroutine(ClearUndockingFlag());
    }

    private IEnumerator ClearUndockingFlag()
    {
        yield return null;
        isUndocking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("boat") && !isDocked && !isUndocking)
        {
            boat = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;

            boatRigidbody = boat.GetComponent<Rigidbody>();
            boatController = boat.GetComponent<BoatController>();
            buoyancyController = boat.GetComponent<BuoyancyController>();
            boatCollider = other; // store the specific collider that entered
            boatIsInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("boat"))
        {
            boatIsInRange = false;

            if (!isDocked && !isUndocking)
            {
                boat = null;
                boatRigidbody = null;
                boatController = null;
                buoyancyController = null;
                boatCollider = null;
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

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(dockSnapTransform.position, undockPromptRadius);
        }

        if (boatSpawnTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(boatSpawnTransform.position, Vector3.one);
            Gizmos.DrawRay(boatSpawnTransform.position, boatSpawnTransform.forward * 3f);
        }

        if (playerSpawnTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(playerSpawnTransform.position, Vector3.one * 0.5f);
            Gizmos.DrawRay(playerSpawnTransform.position, playerSpawnTransform.forward * 2f);
        }
    }
}