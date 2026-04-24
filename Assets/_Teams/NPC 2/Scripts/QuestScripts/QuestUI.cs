using UnityEngine;
using TMPro;

// Programmer: Boas

public class QuestUI : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private Transform questListContent;
    [Header("Quest prefab")]
    [SerializeField] private GameObject questEntryPrefab;

    private void Start()
    {
        // start hidden
        transform.localScale = Vector3.zero;
    }

    // edited by Arch
    private void Update()
    {
        // check if q key is pressed to hide or show the panel
        if (Input.GetKeyDown(KeyCode.G))
        {
            // use scale to hide so the script stays active to listen for keys
            bool isHidden = transform.localScale == Vector3.zero;
            transform.localScale = isHidden ? Vector3.one : Vector3.zero;
        }
    }

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