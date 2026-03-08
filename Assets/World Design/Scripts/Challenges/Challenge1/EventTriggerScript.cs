using UnityEngine;
using UnityEngine.Events;

public class TriggerScript : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string targetTag = "Player"; 
    

    [Header("Events")]
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;
    public GameObject triggerObject;

    private void OnTriggerEnter(Collider other)
    {
        if (GetComponent<Collider>().CompareTag(targetTag))
        {
            Debug.Log($"Entered trigger: {other.name}");
            
            // onTriggerEnter?.Invoke();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"Exited trigger: {other.name}");
            // onTriggerExit?.Invoke();
        }
    }
}
