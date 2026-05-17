using UnityEngine;
using TMPro;

// Programmer: Boas
// Edited by: Arch

public class QuestUI : MonoBehaviour
{
    [Header("Quest list parent")]
    [SerializeField] private Transform questListContent;

    [Header("Quest entry prefab")]
    [SerializeField] private GameObject questEntryPrefab;

    private void Start()
    {
        // Quest display has moved to the journal (QuestJournalPage).
        gameObject.SetActive(false);
    }


    /// <summary>
    /// Legacy method kept so existing callers compile without errors.
    /// Quest display is now handled by QuestJournalPage — this is a no-op.
    /// </summary>
    public void UpdateQuestUI()
    {
        // No-op: QuestJournalPage handles all quest display now.
    }
}
