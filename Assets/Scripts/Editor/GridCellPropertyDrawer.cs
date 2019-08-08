using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GridCell))]
public class GridCellPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var propB = property.FindPropertyRelative("b");
        var propRow = property.FindPropertyRelative("row");
        var propCol = property.FindPropertyRelative("col");

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var scale = 1f / 3f;
        var width = position.width * scale;

        EditorGUIUtility.labelWidth = 30f;

        var rect = new Rect(position.position, new Vector2(width, position.height));

        //base
        EditorGUI.PropertyField(rect, propB);

        //row
        rect.x += rect.width;
        EditorGUI.PropertyField(rect, propRow);

        //col
        rect.x += rect.width;
        EditorGUI.PropertyField(rect, propCol);

        EditorGUIUtility.labelWidth = 0f;
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
