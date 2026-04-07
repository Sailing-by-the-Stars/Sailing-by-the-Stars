using UnityEngine;

public class DoorEndTrigger : MonoBehaviour
{
    [Tooltip("Drag the CreditsManager GameObject here.")]
    [SerializeField] private CreditsManager creditsManager;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            creditsManager.StartCredits();
        }
    }
}