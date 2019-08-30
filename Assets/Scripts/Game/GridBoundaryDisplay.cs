using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBoundaryDisplay : MonoBehaviour {
    public Material material;

    private CameraRenderLines mCamRenderLines;

    void OnDisable() {
        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= RefreshVisible;

        if(mCamRenderLines) {
            mCamRenderLines.Remove(name);
        }
    }

    void OnEnable() {
        if(!mCamRenderLines)
            mCamRenderLines = Camera.main.GetComponent<CameraRenderLines>();

        if(mCamRenderLines) {
            var ctrl = GridEditController.instance.entityContainer.controller;
            var ctrlTrans = ctrl.transform;
            var bounds = ctrl.bounds;

            var min = ctrlTrans.TransformPoint(bounds.min);
            var max = ctrlTrans.TransformPoint(bounds.max);

            var vtx = new Vector3[24];

            //bottom
            vtx[0] = new Vector3(min.x, min.y, min.z); vtx[1] = new Vector3(min.x, min.y, max.z);
            vtx[2] = new Vector3(min.x, min.y, max.z); vtx[3] = new Vector3(max.x, min.y, max.z);
            vtx[4] = new Vector3(max.x, min.y, max.z); vtx[5] = new Vector3(max.x, min.y, min.z);
            vtx[6] = new Vector3(max.x, min.y, min.z); vtx[7] = new Vector3(min.x, min.y, min.z);

            //top
            vtx[8] = new Vector3(min.x, max.y, min.z); vtx[9] = new Vector3(min.x, max.y, max.z);
            vtx[10] = new Vector3(min.x, max.y, max.z); vtx[11] = new Vector3(max.x, max.y, max.z);
            vtx[12] = new Vector3(max.x, max.y, max.z); vtx[13] = new Vector3(max.x, max.y, min.z);
            vtx[14] = new Vector3(max.x, max.y, min.z); vtx[15] = new Vector3(min.x, max.y, min.z);

            //corners
            vtx[16] = new Vector3(min.x, min.y, min.z); vtx[17] = new Vector3(min.x, max.y, min.z);
            vtx[18] = new Vector3(min.x, min.y, max.z); vtx[19] = new Vector3(min.x, max.y, max.z);
            vtx[20] = new Vector3(max.x, min.y, max.z); vtx[21] = new Vector3(max.x, max.y, max.z);
            vtx[22] = new Vector3(max.x, min.y, min.z); vtx[23] = new Vector3(max.x, max.y, min.z);

            mCamRenderLines.Add(name, vtx, material, false);
        }

        RefreshVisible();

        GridEditController.instance.editChangedCallback += RefreshVisible;
    }

    void RefreshVisible() {
        if(!mCamRenderLines)
            return;

        var isVisible = GridEditController.instance.editMode == GridEditController.EditMode.Expand;

        mCamRenderLines.SetVisible(name, isVisible);
    }
}
