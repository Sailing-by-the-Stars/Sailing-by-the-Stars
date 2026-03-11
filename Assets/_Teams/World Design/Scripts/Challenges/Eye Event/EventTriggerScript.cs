using UnityEngine;
using UnityEngine.Events;

namespace World_Design.Scripts.Challenges.Challenge1
{
    public class TriggerScript : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string targetTag = "Player";

        public Animator animator;

        [Header("Events")]
        public UnityEvent onTriggerEnter;
        public UnityEvent onTriggerExit;
        public GameObject triggerObject;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidTrigger(other))
            {
                return;
            }

            Debug.Log($"Entered trigger: {other.name}");
            onTriggerEnter?.Invoke();
            animator.SetTrigger("Activate");
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidTrigger(other))
            {
                return;
            }

            Debug.Log($"Exited trigger: {other.name}");
            onTriggerExit?.Invoke();
        }

        private bool IsValidTrigger(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(targetTag))
            {
                return true;
            }

            return other.CompareTag(targetTag);
        }
    }
}
