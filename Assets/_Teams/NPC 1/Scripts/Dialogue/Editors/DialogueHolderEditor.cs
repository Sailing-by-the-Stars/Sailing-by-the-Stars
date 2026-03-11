using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCDialogueHolder))]
public class NPCDialogueHolderEditor : Editor
{
    SerializedProperty firstDialogue;
    SerializedProperty hasRepeatDialogue;
    SerializedProperty repeatDialogue;
    SerializedProperty interactionDistance;

    void OnEnable()
    {
        firstDialogue = serializedObject.FindProperty("firstDialogue");
        hasRepeatDialogue = serializedObject.FindProperty("hasRepeatDialogue");
        repeatDialogue = serializedObject.FindProperty("repeatDialogue");
        interactionDistance = serializedObject.FindProperty("interactionDistance");
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
        EditorGUILayout.PropertyField(interactionDistance);

        serializedObject.ApplyModifiedProperties();
    }
}