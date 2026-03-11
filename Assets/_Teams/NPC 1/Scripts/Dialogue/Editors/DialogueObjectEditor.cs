using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
// Created By Jantina
[CustomEditor(typeof(Dialogue)), CanEditMultipleObjects]
public class DialogueObjectEditor : Editor
{
    SerializedProperty talkingSpeed_prop;
    SerializedProperty hasName_prop;
    SerializedProperty npcName_prop;
    SerializedProperty nodes_prop;

    Dictionary<int, bool> foldouts = new Dictionary<int, bool>();

    void OnEnable()
    {
        hasName_prop = serializedObject.FindProperty("hasName");
        npcName_prop = serializedObject.FindProperty("npcName");
        nodes_prop = serializedObject.FindProperty("nodes");
        talkingSpeed_prop = serializedObject.FindProperty("talkingSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Character Data", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(hasName_prop, new GUIContent("Does it have a name?"));
        if (hasName_prop.boolValue)
            EditorGUILayout.PropertyField(npcName_prop, new GUIContent("Name"));
        EditorGUILayout.PropertyField(talkingSpeed_prop, new GUIContent("Talking Speed"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add Node", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Dialogue Line")) AddNode(new DialogueLineNode());
        if (GUILayout.Button("Add Choice Node")) AddNode(new ChoiceNode());
        if (GUILayout.Button("Add Conditional Node")) AddNode(new ConditionalNode());
        if (GUILayout.Button("Give a Quest Node")) AddNode(new StartQuestNode());

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialogue Nodes", EditorStyles.boldLabel);
        DrawNodes();
        serializedObject.ApplyModifiedProperties();
    }

    void DrawNodes()
    {
        if (nodes_prop == null) return;

        for (int i = 0; i < nodes_prop.arraySize; i++)
        {
            SerializedProperty element = nodes_prop.GetArrayElementAtIndex(i);
            if (element == null) continue;

            SerializedProperty nodeIDProp = element.FindPropertyRelative("nodeID");
            SerializedProperty textProp = element.FindPropertyRelative("text");

            string header = !string.IsNullOrEmpty(nodeIDProp.stringValue) ? nodeIDProp.stringValue : $"Node {i}";
            if (!foldouts.ContainsKey(i)) foldouts[i] = true;
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], header, true);
            if (!foldouts[i]) continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(nodeIDProp, new GUIContent("Node ID"));
            EditorGUILayout.PropertyField(textProp, new GUIContent("Text"));

            if (element.managedReferenceValue is DialogueLineNode)
            {
                SerializedProperty nextNodeProp = element.FindPropertyRelative("nextNodeID");
                DrawNextNodeDropdown(nextNodeProp, "Next Node");
            }

            if (element.managedReferenceValue is ChoiceNode)
            {
                SerializedProperty choicesProp = element.FindPropertyRelative("choices");
                EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);

                for (int c = 0; c < Mathf.Min(2, choicesProp.arraySize); c++)
                {
                    SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(c);
                    SerializedProperty choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
                    SerializedProperty nextNodeProp = choiceProp.FindPropertyRelative("nextNodeID");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(choiceTextProp, new GUIContent($"Choice {c + 1}"));
                    DrawNextNodeDropdown(nextNodeProp, "Next Node");
                    EditorGUILayout.EndHorizontal();
                }

                if (choicesProp.arraySize < 2 && GUILayout.Button("Add Choice"))
                {
                    choicesProp.arraySize++;
                }
            }

            if (element.managedReferenceValue is ConditionalNode)
            {
                SerializedProperty trueNodeProp = element.FindPropertyRelative("trueNodeID");
                SerializedProperty falseNodeProp = element.FindPropertyRelative("falseNodeID");

                EditorGUILayout.LabelField("Condition Branches", EditorStyles.boldLabel);
                DrawNextNodeDropdown(trueNodeProp, "True Node");
                DrawNextNodeDropdown(falseNodeProp, "False Node");

                SerializedProperty conditionsProp = element.FindPropertyRelative("conditions");
                EditorGUILayout.PropertyField(conditionsProp, true);
            }
            if (element.managedReferenceValue is StartQuestNode)
            {
                SerializedProperty questProp = element.FindPropertyRelative("questToStart");
                SerializedProperty nextNodeProp = element.FindPropertyRelative("nextNodeID");

                EditorGUILayout.LabelField("Quest Settings", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(questProp, new GUIContent("Quest To Start"));

                DrawNextNodeDropdown(nextNodeProp, "Next Node");
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Delete Node"))
            {
                string nodeID = nodeIDProp != null ? nodeIDProp.stringValue : $"Node {i}";
                if (EditorUtility.DisplayDialog("Delete Node?", $"Are you sure you want to delete node {nodeID}?", "Yes", "No"))
                {
                    nodes_prop.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
            }

            EditorGUILayout.EndVertical();
        }
    }

    void AddNode(DialogueNode node)
    {
        serializedObject.Update();
        nodes_prop.arraySize++;
        SerializedProperty element = nodes_prop.GetArrayElementAtIndex(nodes_prop.arraySize - 1);
        element.managedReferenceValue = node;
        serializedObject.ApplyModifiedProperties();
    }

    string[] GetAllNodeIDs()
    {
        var dialogue = (Dialogue)target;
        var list = new List<string>();
        foreach (var node in dialogue.nodes)
            if (node != null && !string.IsNullOrEmpty(node.nodeID))
                list.Add(node.nodeID);
        return list.ToArray();
    }

    void DrawNextNodeDropdown(SerializedProperty property, string label)
    {
        string[] nodeIDs = GetAllNodeIDs();
        string[] options = new string[nodeIDs.Length + 1];
        options[0] = "END";
        for (int i = 0; i < nodeIDs.Length; i++) options[i + 1] = nodeIDs[i];

        int index = System.Array.IndexOf(options, property.stringValue ?? "END");
        if (index < 0) index = 0;

        int newIndex = EditorGUILayout.Popup(label, index, options);
        if (newIndex >= 0)
            property.stringValue = options[newIndex] == "END" ? null : options[newIndex];
    }
}