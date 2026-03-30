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

        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Draw condition type
        EditorGUI.PropertyField(line, conditionType);

        line.y += EditorGUIUtility.singleLineHeight + 2;

        ConditionType type = (ConditionType)conditionType.enumValueIndex;

        if (type == ConditionType.Quest)
        {
            EditorGUI.PropertyField(line, questID);
        }
        else if (type == ConditionType.Item)
        {
            EditorGUI.PropertyField(line, itemID);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2) * 2;
    }
}