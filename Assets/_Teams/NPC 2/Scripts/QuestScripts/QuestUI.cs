using UnityEngine;
using TMPro;

// Programmer: Boas

public class QuestUI : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private Transform questListContent;
    [Header("Quest prefab")]
    [SerializeField] private GameObject questEntryPrefab;

    /// <summary>
    ///  Updates quest UI so it shows all current quests.
    /// </summary>
    public void UpdateQuestUI()
    {
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var quest in QuestManager.Instance.ActiveQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            TMP_Text questObjectiveText = entry.transform.Find("QuestObjective").GetComponent<TMP_Text>();

            questNameText.text = quest.Quest.QuestName;

            string objectiveText = "";

            foreach (var obj in quest.Objectives)
            {
                objectiveText += $"{obj.Description} ({obj.CurrentAmount}/{obj.RequiredAmount})\n";
            }

            questObjectiveText.text = objectiveText;
        }
    }
}