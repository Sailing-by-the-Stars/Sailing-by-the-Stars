using UnityEngine;

public class QuestStarter : MonoBehaviour
{
    public void StartQuest(Quest quest)
    {
        Debug.Log("Starting quest: " + quest.QuestName);
        QuestManager.Instance.StartQuest(quest);
    }
}