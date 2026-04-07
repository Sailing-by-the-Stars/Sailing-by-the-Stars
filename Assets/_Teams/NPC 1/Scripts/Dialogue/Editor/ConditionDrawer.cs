using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueCondition))]
public class ConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var conditionType = property.FindPropertyRelative("conditionType");
        var questID = property.FindPropertyRelative("questID");
        var itemID = property.FindPropertyRelative("itemID");
        var dialogue = property.FindPropertyRelative("dialogue"); // 👈 NEW

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Condition type dropdown
        EditorGUI.PropertyField(line, conditionType);

        line.y += EditorGUIUtility.singleLineHeight + 2;

        ConditionType type = (ConditionType)conditionType.enumValueIndex;

        switch (type)
        {
            case ConditionType.Quest:
                EditorGUI.PropertyField(line, questID, new GUIContent("Quest ID"));
                break;

            case ConditionType.Item:
                EditorGUI.PropertyField(line, itemID, new GUIContent("Item ID"));
                break;

            case ConditionType.Dialogue:
                EditorGUI.PropertyField(line, dialogue, new GUIContent("Dialogue")); // 👈 NEW
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2) * 2;
    }
}