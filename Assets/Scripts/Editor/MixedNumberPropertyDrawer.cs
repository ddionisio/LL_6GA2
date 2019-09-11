using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MixedNumber))]
public class MixedNumberPropertyDrawer : PropertyDrawer {
    private int[] mSignVals = new int[] { 0, 1 };
    private string[] mSigns = new string[] { "+", "-" };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        //hack
        if(position.width < 350f) {
            position.x -= 60;
            position.width += 60;
        }

        var propSign = property.FindPropertyRelative("_negative");
        var propWhole = property.FindPropertyRelative("_whole");
        var propNumerator = property.FindPropertyRelative("_numerator");
        var propDenominator = property.FindPropertyRelative("_denominator");

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var scale = 1f / 4f;
        var width = position.width * scale;

        //EditorGUIUtility.labelWidth = 30f;

        //sign
        var rect = new Rect(position.position, new Vector2(width, position.height));

        int curSignInd = propSign.boolValue ? 1 : 0;
        curSignInd = EditorGUI.IntPopup(rect, curSignInd, mSigns, mSignVals);
        propSign.boolValue = curSignInd == 1;

        //whole
        rect.x += rect.width;
        var wholeVal = EditorGUI.IntField(rect, propWhole.intValue);
        if(wholeVal < 0) propSign.boolValue = true;
        propWhole.intValue = Mathf.Abs(wholeVal);

        //fraction
        rect.x += rect.width;
        var numeratorVal = EditorGUI.IntField(rect, propNumerator.intValue);
        if(numeratorVal < 0) propSign.boolValue = true;
        propNumerator.intValue = Mathf.Abs(numeratorVal);

        rect.x += rect.width;
        var denominatorVal = EditorGUI.IntField(rect, propDenominator.intValue);
        if(denominatorVal < 0) propSign.boolValue = true;
        propDenominator.intValue = Mathf.Abs(denominatorVal);

        EditorGUIUtility.labelWidth = 0f;
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
