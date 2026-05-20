using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Programmer: Arch


public class QuestJournalPage : MonoBehaviour
{
    public static QuestJournalPage Instance { get; private set; }

    // ── Render Textures ──────────────────────────────────────────────────────
    [Header("Render Textures (auto-created at 1240x1754 if left empty)")]
    [SerializeField] private RenderTexture activeQuestRT;
    [SerializeField] private RenderTexture completedQuestRT;

    // ── Cameras ──────────────────────────────────────────────────────────────
    [Header("Quest Cameras (each renders one canvas to one RenderTexture)")]
    [SerializeField] private Camera activeQuestCamera;
    [SerializeField] private Camera completedQuestCamera;

    // ── Canvas content roots ─────────────────────────────────────────────────
    [Header("Active Quests Canvas Content (ScrollView > Viewport > Content)")]
    [SerializeField] private Transform activeQuestContent;

    [Header("Completed Quests Canvas Content (ScrollView > Viewport > Content)")]
    [SerializeField] private Transform completedQuestContent;

    // ── Prefabs ───────────────────────────────────────────────────────────────
    [Header("Quest Entry Prefab (must have children named QuestName and QuestObjective)")]
    [SerializeField] private GameObject questEntryPrefab;

    // ── Journal ───────────────────────────────────────────────────────────────
    [Header("Journal reference (auto-found if empty)")]
    [SerializeField] private Journal journal;
    [Header("Scroll Controls")]
    [SerializeField] private float scrollSpeed = 200f;

    private void Update()
    {
        ScrollWithKeys(activeQuestContent, KeyCode.UpArrow, KeyCode.DownArrow);
        ScrollWithKeys(completedQuestContent, KeyCode.LeftArrow, KeyCode.RightArrow);
    }

    private void ScrollWithKeys(Transform content, KeyCode upKey, KeyCode downKey)
    {
        if (content == null) return;

        UnityEngine.UI.ScrollRect scrollRect = 
            content.GetComponentInParent<UnityEngine.UI.ScrollRect>();

        if (scrollRect == null) return;

        if (Input.GetKey(upKey))
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition + scrollSpeed * Time.deltaTime / 1000f);

        if (Input.GetKey(downKey))
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition - scrollSpeed * Time.deltaTime / 1000f);
    }

    // ── Constants ─────────────────────────────────────────────────────────────
    private const int PageWidth  = 1240;
    private const int PageHeight = 1754;
    private const int DepthBits  = 16;

    // Completed quests
    private readonly List<(string id, string questName)> completedQuestLog = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateRenderTextures();
        AssignCameraTargets();
    }

    private void Start()
    {
        if (journal == null)
            journal = FindFirstObjectByType<Journal>();

        RegisterJournalPage();

        // Show empty state before any quests are active
        RefreshActivePanel(new List<QuestProgress>());
        RefreshCompletedPanel();
    }


    /// <summary>
    /// Call this whenever the quest state changes. Rebuilds both journal pages.
    /// completedQuestId and completedQuestName are only needed when a quest just finished.
    /// </summary>
    public void UpdateDisplay(
        IReadOnlyList<QuestProgress> activeQuests,
        string completedQuestId   = "",
        string completedQuestName = "")
    {
        TryLogCompletedQuest(completedQuestId, completedQuestName);
        RefreshActivePanel(activeQuests);
        RefreshCompletedPanel();
    }


    private void CreateRenderTextures()
    {
        if (activeQuestRT == null)
        {
            activeQuestRT      = new RenderTexture(PageWidth, PageHeight, DepthBits);
            activeQuestRT.name = "ActiveQuestRT";
        }

        if (completedQuestRT == null)
        {
            completedQuestRT      = new RenderTexture(PageWidth, PageHeight, DepthBits);
            completedQuestRT.name = "CompletedQuestRT";
        }
    }

    private void AssignCameraTargets()
    {
        if (activeQuestCamera != null)
            activeQuestCamera.targetTexture = activeQuestRT;

        if (completedQuestCamera != null)
            completedQuestCamera.targetTexture = completedQuestRT;
    }

    private void RegisterJournalPage()
    {
        Page questPage = new Page
        {
            leftPage  = activeQuestRT,
            rightPage = completedQuestRT
        };

        // pageIDHack = -1 appends to the end (matches JournalPagePickup default behaviour)
        //journal.AddPage(questPage, -1);
    }

    private void TryLogCompletedQuest(string completedQuestId, string completedQuestName)
    {
        bool isValidEntry    = !string.IsNullOrEmpty(completedQuestId);
        bool alreadyRecorded = completedQuestLog.Exists(q => q.id == completedQuestId);

        if (isValidEntry && !alreadyRecorded)
            completedQuestLog.Add((completedQuestId, completedQuestName));
    }

    /// <summary>
    /// Rebuilds the active-quests canvas (displayed on the left journal page).
    /// </summary>
    private void RefreshActivePanel(IReadOnlyList<QuestProgress> activeQuests)
    {
        if (activeQuestContent == null)
            return;

        ClearChildren(activeQuestContent);

        if (activeQuests.Count == 0)
        {
            SpawnEntry(activeQuestContent, "No active quests.", "");
            return;
        }

        foreach (QuestProgress quest in activeQuests)
        {
            string objectives = BuildObjectiveText(quest);
            SpawnEntry(activeQuestContent, quest.Quest.QuestName, objectives);
        }
    }

    /// <summary>
    /// Rebuilds the completed-quests canvas (displayed on the right journal page).
    /// </summary>
    private void RefreshCompletedPanel()
    {
        if (completedQuestContent == null)
            return;

        ClearChildren(completedQuestContent);

        if (completedQuestLog.Count == 0)
        {
            SpawnEntry(completedQuestContent, "None yet.", "");
            return;
        }

        foreach ((string id, string questName) in completedQuestLog)
        {
            SpawnEntry(completedQuestContent, questName, "Completed");
        }
    }

    private string BuildObjectiveText(QuestProgress quest)
    {
        string result = "";

        foreach (QuestObjective obj in quest.Objectives)
            result += $"- {obj.Description} ({obj.CurrentAmount}/{obj.RequiredAmount})\n";

        return result;
    }

    private void SpawnEntry(Transform parent, string title, string body)
    {
        GameObject entry = Instantiate(questEntryPrefab, parent);

        // Match the child-naming convention used in the existing QuestEntry prefab
        SetText(entry, "QuestName",      title);
        SetText(entry, "QuestObjective", body);
    }

    private void SetText(GameObject entry, string childName, string text)
    {
        Transform child = entry.transform.Find(childName);

        if (child == null)
        {
            Debug.LogWarning($"QuestJournalPage: could not find child '{childName}' on entry prefab.");
            return;
        }

        child.GetComponent<TMP_Text>().text = text;
    }

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }
}
