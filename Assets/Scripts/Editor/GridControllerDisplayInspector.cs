using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridController))]
public class GridControllerInspector : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();

        var dat = target as GridController;

        if(GUILayout.Button("Refresh Collider")) {
            var coll = dat.GetComponent<BoxCollider>();
            if(coll) {
                var bound = dat.bounds;
                coll.size = new Vector3(bound.size.x, GridController.boxColliderHeight, bound.size.z);
                coll.center = new Vector3(0f, -GridController.boxColliderHeight * 0.5f, 0f);
            }
        }
    }
}
