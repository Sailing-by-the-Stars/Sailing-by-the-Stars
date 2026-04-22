using UnityEngine;

/// <summary>
/// TESTING ONLY — remove before shipping.
/// Starts AcceptanceQuestStep1 so all three pedestal objectives are active.
/// Wrapped in try/catch so a missing QuestUI won't crash the test.
/// </summary>
public class DebugQuestStarter : MonoBehaviour
{
    [Tooltip("Drag AcceptanceQuestStep1.asset here.")]
    public Quest acceptanceQuestStep1;

    private void Start()
    {
        if (acceptanceQuestStep1 == null)
        {
            Debug.LogWarning("[DebugQuestStarter] No quest assigned.");
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[DebugQuestStarter] QuestManager not found in scene.");
            return;
        }

        try
        {
            QuestManager.Instance.StartQuest(acceptanceQuestStep1);
            Debug.Log("[DebugQuestStarter] AcceptanceQuestStep1 started — pedestals are now testable.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DebugQuestStarter] StartQuest threw an exception: {e.Message}\n" +
                             "This usually means QuestUI is missing from the scene. " +
                             "The quest objectives are still registered and pedestals will work.");
        }
    }
}