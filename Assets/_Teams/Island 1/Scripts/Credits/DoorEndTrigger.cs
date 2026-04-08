using UnityEngine;

public class DoorEndTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            CreditsManager.Instance.StartCredits();
        }
    }
}