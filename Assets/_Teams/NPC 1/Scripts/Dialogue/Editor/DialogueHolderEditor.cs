using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCDialogueHolder))]
public class NPCDialogueHolderEditor : Editor
{
    SerializedProperty firstDialogue;
    SerializedProperty hasRepeatDialogue;
    SerializedProperty repeatDialogue;
    SerializedProperty interactionMessage;

    void OnEnable()
    {
        firstDialogue = serializedObject.FindProperty("firstDialogue");
        hasRepeatDialogue = serializedObject.FindProperty("hasRepeatDialogue");
        repeatDialogue = serializedObject.FindProperty("repeatDialogue");
        interactionMessage = serializedObject.FindProperty("interactMessage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Dialogue", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(firstDialogue);
        EditorGUILayout.PropertyField(hasRepeatDialogue);

        if (hasRepeatDialogue.boolValue)
        {
            EditorGUILayout.PropertyField(repeatDialogue);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(interactionMessage);

        serializedObject.ApplyModifiedProperties();
    }
}