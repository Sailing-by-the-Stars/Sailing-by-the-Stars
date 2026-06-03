// Created by: Arch

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class NotePageGroup
{
    [Header("Group identifier — must match DialogueNoteData pageGroupID")]
    public string groupID;

    [Header("Camera and display references")]
    [SerializeField] private Camera notesCamera;
    [SerializeField] private RenderTexture notesRT;
    [SerializeField] private TMP_Text notesText;

    [HideInInspector] public bool pageRegistered;
    [HideInInspector] public List<string> notes = new();

    public Camera NotesCamera => notesCamera;
    public RenderTexture NotesRT
    {
        get => notesRT;
        set => notesRT = value;
    }
    public TMP_Text NotesText => notesText;
}

public class DialogueNotesPage : MonoBehaviour
{
    public static DialogueNotesPage Instance { get; private set; }

    [Header("One entry per NPC — groupID must match DialogueNoteData pageGroupID")]
    [SerializeField] private List<NotePageGroup> groups;

    [Header("All DialogueNoteData assets to listen for")]
    [SerializeField] private List<DialogueNoteData> noteDataList;

    [Header("Journal reference (auto-found if empty)")]
    [SerializeField] private Journal journal;

    private const int PageWidth  = 1240;
    private const int PageHeight = 1754;
    private const int DepthBits  = 16;

    private readonly HashSet<Dialogue> processedDialogues = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (NotePageGroup group in groups)
        {
            if (group.NotesRT == null)
            {
                group.NotesRT = new RenderTexture(PageWidth, PageHeight, DepthBits);
                group.NotesRT.name = $"DialogueNotesRT_{group.groupID}";
            }

            if (group.NotesCamera != null)
                group.NotesCamera.targetTexture = group.NotesRT;
        }
    }

    private void Start()
    {
        // Include inactive: the journal book is a pickup tool that can be inactive at scene start.
        if (journal == null)
            journal = FindFirstObjectByType<Journal>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        if (PlayerState.Instance == null) return;

        foreach (Dialogue dialogue in PlayerState.Instance.completedDialogues)
        {
            if (processedDialogues.Contains(dialogue)) continue;
            processedDialogues.Add(dialogue);
            CheckForNote(dialogue);
        }
    }

    private void CheckForNote(Dialogue dialogue)
    {
        foreach (DialogueNoteData data in noteDataList)
        {
            if (data.Dialogue == dialogue)
            {
                AddNoteToGroup(data.PageGroupID, data.NoteText);
                return;
            }
        }
    }

    // The most recently created spread that only has a left page so far.
    // The next note to arrive will fill its right page instead of creating a new spread.
    private Page lastUnpairedPage = null;

    private void AddNoteToGroup(string groupID, string text)
    {
        NotePageGroup group = groups.Find(g => g.groupID == groupID);

        if (group == null)
        {
            Debug.LogWarning($"DialogueNotesPage: no group with ID '{groupID}'");
            return;
        }

        if (group.pageRegistered) return;

        group.notes.Add(text);
        RefreshGroupText(group);
        group.pageRegistered = true;

        RegisterPagePaired(group.NotesRT);
    }

    /// Pairs notes two per journal spread: odd notes go on the left, even notes fill the right.
    private void RegisterPagePaired(RenderTexture noteRT)
    {
        if (journal == null)
            journal = FindFirstObjectByType<Journal>(FindObjectsInactive.Include);

        if (journal == null)
        {
            Debug.LogError("DialogueNotesPage: no Journal found in the scene — cannot add note page.");
            return;
        }

        JournalSection questSection = journal.GetSection(SectionName.Quests);

        if (questSection == null)
        {
            Debug.LogError("DialogueNotesPage: Journal has no Quests section — cannot add note page.");
            return;
        }

        if (lastUnpairedPage == null)
        {
            // Odd note — start a new spread with this note on the left page.
            lastUnpairedPage = new Page { leftPage = noteRT, rightPage = null, pageID = -1 };
            questSection.AddPage(lastUnpairedPage, -1);
        }
        else
        {
            // Even note — fill the right page of the existing spread. No new page added.
            lastUnpairedPage.rightPage = noteRT;
            lastUnpairedPage = null;
        }
    }

    private void RefreshGroupText(NotePageGroup group)
    {
        if (group.NotesText == null) return;
        group.NotesText.text = string.Join("\n\n---\n\n", group.notes);
    }
}
