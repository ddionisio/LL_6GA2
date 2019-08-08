using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridEntity))]
public class GridEntityInspector : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();

        var dat = target as GridEntity;

        var cellInd = dat.cellIndex;

        EditorGUILayout.LabelField("Grid Cell", string.Format("base = {0}, row = {1}, col = {2}", cellInd.b, cellInd.row, cellInd.col));
    }
}
