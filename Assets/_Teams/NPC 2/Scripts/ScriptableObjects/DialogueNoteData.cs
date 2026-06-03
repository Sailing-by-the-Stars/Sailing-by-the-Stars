// Created by: Arch

using UnityEngine;

/// Maps one Dialogue asset to a journal page group and the note text to display when that dialogue completes.
[CreateAssetMenu(fileName = "DialogueNote", menuName = "NPC2/Dialogue Note")]
public class DialogueNoteData : ScriptableObject
{
    [Header("Dialogue that triggers this note")]
    [SerializeField] private Dialogue dialogue;

    [Header("Must match a groupID on DialogueNotesPage (e.g. Denial, Acceptance)")]
    [SerializeField] private string pageGroupID;

    [Header("Note text shown in the journal")]
    [TextArea(3, 10)]
    [SerializeField] private string noteText;

    public Dialogue Dialogue => dialogue;
    public string PageGroupID => pageGroupID;
    public string NoteText => noteText;
}
