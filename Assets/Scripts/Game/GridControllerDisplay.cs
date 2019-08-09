using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridControllerDisplay : MonoBehaviour {
    [Header("Data")]
    public GridController gridControl;
    public MeshFilter gridMeshFilter;

    [Header("Config")]
    public float textureTile = 1f;
    public bool refreshOnEnable = true;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    //order: starting lower left, clockwise
    private Vector3[] mVtx = new Vector3[4];
    private Vector2[] mUVs = new Vector2[4];
    private int[] mInds = new int[] { 0, 1, 2, 2, 3, 0 };

    private int mGridRow = -1, mGridCol = -1;
    private float mUnitSize = 0f;

    public void RefreshMesh(bool forceRefresh) {
        if(!gridControl || !gridMeshFilter)
            return;

        var mesh = gridMeshFilter.sharedMesh;
        if(!mesh)
            return;

        var cellSize = gridControl.cellSize;

        if(!forceRefresh && mGridRow == cellSize.row && mGridCol == cellSize.col && mUnitSize == gridControl.unitSize)
            return;
                
        mGridRow = cellSize.row;
        mGridCol = cellSize.col;
        mUnitSize = gridControl.unitSize;

        if(mUnitSize <= 0f || mGridRow <= 0 || mGridCol <= 0)
            return;
                
        mesh.Clear();

        var bounds = gridControl.bounds;

        mVtx[0] = new Vector3(-bounds.extents.x, 0f, -bounds.extents.z);
        mVtx[1] = new Vector3(-bounds.extents.x, 0f, bounds.extents.z);
        mVtx[2] = new Vector3(bounds.extents.x, 0f, bounds.extents.z);
        mVtx[3] = new Vector3(bounds.extents.x, 0f, -bounds.extents.z);

        //var uvUnit = 1f;// textureTile / mUnitSize;

        var uvSize = new Vector2(textureTile * mGridCol, textureTile * mGridRow);

        mUVs[0] = new Vector2(0f, 0f);
        mUVs[1] = new Vector2(0f, uvSize.y);
        mUVs[2] = new Vector2(uvSize.x, uvSize.y);
        mUVs[3] = new Vector2(uvSize.x, 0f);

        mesh.vertices = mVtx;
        mesh.uv = mUVs;
        mesh.triangles = mInds;
    }

    void OnEnable() {
        if(refreshOnEnable)
            RefreshMesh(false);
    }
}
