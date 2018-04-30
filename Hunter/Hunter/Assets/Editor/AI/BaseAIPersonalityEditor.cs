using Hunter.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseAIPersonality))]
public class BaseAIPersonalityEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * (property.isExpanded ? 12f : 1f);
    }
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

        if (property.isExpanded)
        {
            // Don't make child fields be indented

            EditorGUI.indentLevel++;
            var drawRect = position;
            drawRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(drawRect, new GUIContent("Detection Settings"), EditorStyles.boldLabel);
            drawRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(drawRect, property.FindPropertyRelative("NeedDetectionRange"), new GUIContent("Detection Range"), true);
            drawRect.y += EditorGUIUtility.singleLineHeight * 2f;
            EditorGUI.LabelField(drawRect, new GUIContent("Hunger Settings"), EditorStyles.boldLabel);
            drawRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(drawRect, property.FindPropertyRelative("HungryThresholdRange"), new GUIContent("Hungry Threshold Range"), true);
            drawRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(drawRect, property.FindPropertyRelative("HungryDecayRange"), new GUIContent("Hungry Decay Range"), true);
            drawRect.y += EditorGUIUtility.singleLineHeight*2f;
            EditorGUI.LabelField(drawRect, new GUIContent("Health Settings"), EditorStyles.boldLabel);
            drawRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(drawRect, property.FindPropertyRelative("HealthDecayRange"), new GUIContent("Health Decay Range"), true);
            drawRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(drawRect, property.FindPropertyRelative("DecayHealthWhenStarving"), new GUIContent("Decay Health When Starving"), true);
            EditorGUI.indentLevel--;


        }
        EditorGUI.EndProperty();
    }
}
