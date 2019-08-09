using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridControllerDisplay))]
public class GridControllerDisplayInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();

        var dat = target as GridControllerDisplay;

        if(GUILayout.Button("Refresh Mesh"))
            dat.RefreshMesh(true);
    }
}
