using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridEntityData))]
public class GridEntityDataInspector : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(GUI.changed) {
            var dat = target as GridEntityData;

            dat.RefreshShaderPropertyIds();
        }
    }
}
