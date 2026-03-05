using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Dialogue)), CanEditMultipleObjects]
public class DialogueObjectEditor : Editor
{
    public SerializedProperty talkingspeed_prop,hasname_prop, npcname_prop, dialoguetext_prop, givesquest_prop, quest_prop;

    void OnEnable()
    {
        hasname_prop = serializedObject.FindProperty("hasName");
        npcname_prop = serializedObject.FindProperty("npcName");
        dialoguetext_prop = serializedObject.FindProperty("dialogueLines");
        talkingspeed_prop = serializedObject.FindProperty("talkingSpeed");
        givesquest_prop = serializedObject.FindProperty("givesQuest");
        quest_prop = serializedObject.FindProperty("quest");
    }
    public override void OnInspectorGUI() {
		serializedObject.Update ();
        EditorGUILayout.LabelField("Character Data", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField(hasname_prop, new GUIContent("Does it have a name?"));
        if (hasname_prop.boolValue == true){
            EditorGUILayout.PropertyField( npcname_prop, new GUIContent("Name:") );
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialogue", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField( dialoguetext_prop, new GUIContent("Dialogue lines."));
        EditorGUILayout.PropertyField( talkingspeed_prop, new GUIContent("Talking Speed"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quests", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField( givesquest_prop,new GUIContent("Does it give a quest?") );
        if (givesquest_prop.boolValue == true){
            EditorGUILayout.PropertyField( quest_prop );
        }

        serializedObject.ApplyModifiedProperties ();
    }
}
