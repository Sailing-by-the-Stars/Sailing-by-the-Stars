// using UnityEngine;
// using UnityEditor;
// // Created by Jantina

// [CustomEditor(typeof(DialogueUIManager))]
// public class DialogueUIManagerEditor : Editor
// {
//     private SerializedProperty dialogueBoxProp;
//     private SerializedProperty npcNameTextProp;
//     private SerializedProperty dialogueTextProp;
//     private SerializedProperty nextButtonProp;
//     private SerializedProperty choiceButton1Prop;
//     private SerializedProperty choiceButton2Prop;

//     private void OnEnable()
//     {
//         dialogueBoxProp = serializedObject.FindProperty("dialogueBox");
//         npcNameTextProp = serializedObject.FindProperty("npcNameText");
//         dialogueTextProp = serializedObject.FindProperty("dialogueText");
//         nextButtonProp = serializedObject.FindProperty("nextButton");
//         choiceButton1Prop = serializedObject.FindProperty("choiceButton1");
//         choiceButton2Prop = serializedObject.FindProperty("choiceButton2");
//     }

//     public override void OnInspectorGUI()
//     {
//         serializedObject.Update();

//         EditorGUILayout.LabelField("UI References", EditorStyles.boldLabel);
//         EditorGUILayout.Space();
//         DrawPropertyWithWarning(dialogueBoxProp, "Main panel containing all dialogue UI elements.");
//         DrawPropertyWithWarning(npcNameTextProp, "Text component displaying NPC name.");
//         DrawPropertyWithWarning(dialogueTextProp, "Text component displaying dialogue text.");
//         DrawPropertyWithWarning(nextButtonProp, "Button to advance to the next dialogue line.");
//         DrawPropertyWithWarning(choiceButton1Prop, "First choice button for player selection.");
//         DrawPropertyWithWarning(choiceButton2Prop, "Second choice button for player selection.");

//         serializedObject.ApplyModifiedProperties();
//     }

//     private void DrawPropertyWithWarning(SerializedProperty prop, string tooltip)
//     {
//         EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName, tooltip));
//         if (prop.objectReferenceValue == null)
//         {
//             EditorGUILayout.HelpBox($"Warning: {prop.displayName} is not assigned!", MessageType.Warning);
//         }
//     }
// }