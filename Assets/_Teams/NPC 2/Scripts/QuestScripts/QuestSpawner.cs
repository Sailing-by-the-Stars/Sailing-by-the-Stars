using UnityEngine;
using UnityEngine.InputSystem;

// Programmer: Boas

public class QuestSpawner : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private Quest[] availableQuests;

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (availableQuests.Length == 0) return;

            int index = Random.Range(0, availableQuests.Length);
            Quest selectedQuest = availableQuests[index];

            QuestManager.Instance.StartQuest(selectedQuest);
        }
    }
}

// private void AddRandomQuest()
// {
//     if (availableQuests.Length == 0) return;

//     int index = Random.Range(0, availableQuests.Length);
//     Quest selectedQuest = availableQuests[index];

//     QuestManager.Instance?.AddQuest(selectedQuest);
// }
