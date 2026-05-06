using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Place this on a GameObject with a Trigger Collider spanning the boundary
/// the player must not cross on the Denial Island.
///
/// When the player walks through it for the first time:
///   1. Screen fades to black
///   2. Player is teleported back to the respawn point
///   3. Blockade 1 is disabled
///   4. Blockade 2 is enabled
///   5. The WakeUp blink animation plays, as if they're waking up again
///   6. journal is enabled
///
/// After that the trigger does nothing, so the player can pass freely.
///
/// INSPECTOR SETUP:
///   Respawn Point  — empty GameObject placed at the island start
///   Fade Panel     — a full-screen black UI Image (same one used elsewhere,
///                    or create a new Canvas + Image for this scene)
///   Wake Up        — drag the Eyelids GameObject (with the WakeUp script) here
///   Fade Duration  — how long the fade to black takes (default 0.8s)
/// </summary>
[RequireComponent(typeof(Collider))]
public class PointOfNoReturn : MonoBehaviour
{
    [Header("Teleport")]
    [Tooltip("The position and rotation the player is sent back to.")]
    public Transform respawnPoint;

    [Header("Fade")]
    [Tooltip("A full-screen black UI Image used to fade to black.")]
    public Image fadePanel;

    [Tooltip("How long the fade to black takes in seconds.")]
    public float fadeDuration = 0.8f;

    [Header("Wake Up")]
    [Tooltip("Drag the Eyelids GameObject (the one with the WakeUp script) here.")]
    public WakeUp wakeUp;

    [Header("Rock Blockades")]
    [Tooltip("This blockade is active at the start of the game and disabled after respawn.")]
    public GameObject blockade1;

    [Tooltip("This blockade is inactive at the start of the game and enabled after respawn.")]
    public GameObject blockade2;

    [Header("Journals")]
    [Tooltip("This journal is desabled at the start of the game and enabled after respawn.")]
    public GameObject journal1;

    [Tooltip("This journal is desabled at the start of the game and enabled after respawn.")]
    public GameObject journal2;

    private bool _hasTriggered = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        // Start with the fade panel invisible
        if (fadePanel != null)
            SetFadeAlpha(0f);

        // Start state:
        // Blockade 1 blocks the first path.
        // Blockade 2 is disabled until the player respawns.
        SetInitialBlockadeState();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        if (respawnPoint == null)
        {
            Debug.LogWarning("[PointOfNoReturn] No respawn point assigned!", this);
            return;
        }

        _hasTriggered = true;
        StartCoroutine(ReturnSequence(other.transform));
    }

    private IEnumerator ReturnSequence(Transform player)
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                SetFadeAlpha(Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            SetFadeAlpha(1f);
        }

        Rigidbody rb       = player.GetComponent<Rigidbody>();
        Movement  movement = player.GetComponent<Movement>();

        if (movement != null) movement.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.position    = respawnPoint.position;
            rb.rotation    = respawnPoint.rotation;
        }
        else
        {
            // Fallback – older CharacterController setup
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
            if (cc != null) cc.enabled = true;
        }

        // Wait one fixed-update so the engine registers the new position before
        // we hand control back to the physics solver
        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.isKinematic      = false;
            rb.linearVelocity   = Vector3.zero;
            rb.angularVelocity  = Vector3.zero;
        }

        // Switch the rock blockades after the player has respawned
        if (blockade1 != null)
            blockade1.SetActive(false);

        if (blockade2 != null)
            blockade2.SetActive(true);

        if (journal1 != null)
            journal1.SetActive(true);

        if (journal2 != null)
            journal2.SetActive(true);

        if (movement != null) movement.enabled = true;

        yield return new WaitForSecondsRealtime(0.2f);

        if (fadePanel != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                SetFadeAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            SetFadeAlpha(0f);
            fadePanel.gameObject.SetActive(false);
        }

        // Replay the wake-up blink animation
        if (wakeUp != null)
            wakeUp.Play();
        else
            Debug.LogWarning("[PointOfNoReturn] No WakeUp reference assigned — blink skipped.", this);

        Debug.Log("[PointOfNoReturn] Player returned to island start.");
    }

    private void SetInitialBlockadeState()
    {
        if (blockade1 != null)
            blockade1.SetActive(true);

        if (blockade2 != null)
            blockade2.SetActive(false);
        
        if (journal1 != null)
            journal1.SetActive(false);

        if (journal2 != null)
            journal2.SetActive(false);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadePanel == null) return;
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = _hasTriggered
            ? new Color(0f, 1f, 0f, 0.3f)
            : new Color(1f, 0f, 0f, 0.3f);

        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
        else
            Gizmos.DrawSphere(Vector3.zero, 2f);
    }
}
