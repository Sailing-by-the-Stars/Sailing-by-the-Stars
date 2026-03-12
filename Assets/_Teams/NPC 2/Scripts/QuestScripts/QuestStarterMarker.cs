using UnityEngine;

// Programmer: Boas

public class QuestStarterMarker : MonoBehaviour
{
    [SerializeField] private Quest questToStart;

    /// <summary>
    /// Activates when player entars collision
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (questToStart != null)
            {
                QuestManager.Instance.StartQuest(questToStart);
                Destroy(gameObject);
            }
        }
    }
}