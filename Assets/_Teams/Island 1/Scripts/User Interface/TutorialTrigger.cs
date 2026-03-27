using Assets._Teams.Island_1.Scripts.User_Interface;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialStep[] stepsToTrigger;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        TutorialManager.Instance.EnqueueSteps(stepsToTrigger);
        triggered = true;
    }
}
