using UnityEngine;

public class TutorialHitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialSequence.Instance.NextStep(0);
            Destroy(gameObject);
        }
    }
}
