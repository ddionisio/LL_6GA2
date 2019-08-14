using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityDisplayFloor : MonoBehaviour, M8.IPoolSpawnComplete {
    [Header("Data")]
    public GridEntity gridEntity;
    
    [Header("Display")]
    public MeshFilter gridMeshFilter;
    public Renderer gridRenderer;
    
    [Header("Config")]
    public float textureTile = 1f;
    public float alpha = 0.5f;
    public bool refreshOnEnable = false; //set this to true for non-placeables
    public bool hideOnEnable = false; //set this to true for non-placeables

    public bool isVisible {
        get { return mIsVisible; }
        set {
            if(mIsVisible != value) {
                mIsVisible = value;
                gridRenderer.enabled = mIsVisible;
            }
        }
    }

    private static readonly int[] mInds = new int[] { 0, 1, 2, 2, 3, 0 };

    //order: starting lower left, clockwise
    private Vector3[] mVtx = new Vector3[4];
    private Vector2[] mUVs = new Vector2[4];
    
    private int mGridRowCount = -1, mGridColCount = -1;

    private Material mMat;

    private bool mIsVisible;

    public void RefreshColor() {
        if(!mMat || !gridEntity || !gridEntity.data)
            return;

        var clr = gridEntity.data.color;
        clr.a = alpha;

        mMat.SetColor(gridEntity.data.shaderColorId, clr);
    }

    public void RefreshMesh(bool forceRefresh) {
        if(!gridEntity || !gridMeshFilter)
            return;

        //generate mesh if not set
        var mesh = gridMeshFilter.sharedMesh;
        if(!mesh) {
            mesh = new Mesh();
            gridMeshFilter.sharedMesh = mesh;
        }

        var cellSize = gridEntity.cellSize;

        if(!forceRefresh && mGridRowCount == cellSize.row && mGridColCount == cellSize.col)
            return;

        mGridRowCount = cellSize.row;
        mGridColCount = cellSize.col;

        if(mGridRowCount <= 0 || mGridColCount <= 0)
            return;

        var bounds = gridEntity.bounds;

        mVtx[0] = new Vector3(-bounds.extents.x, 0f, -bounds.extents.z);
        mVtx[1] = new Vector3(-bounds.extents.x, 0f, bounds.extents.z);
        mVtx[2] = new Vector3(bounds.extents.x, 0f, bounds.extents.z);
        mVtx[3] = new Vector3(bounds.extents.x, 0f, -bounds.extents.z);

        //var uvUnit = 1f;// textureTile / mUnitSize;

        var uvSize = new Vector2(textureTile * mGridColCount, textureTile * mGridRowCount);

        mUVs[0] = new Vector2(0f, 0f);
        mUVs[1] = new Vector2(0f, uvSize.y);
        mUVs[2] = new Vector2(uvSize.x, uvSize.y);
        mUVs[3] = new Vector2(uvSize.x, 0f);

        if(mesh.vertexCount != mVtx.Length)
            mesh.Clear();

        mesh.vertices = mVtx;
        mesh.uv = mUVs;
        mesh.triangles = mInds;
    }

    void M8.IPoolSpawnComplete.OnSpawnComplete() {
        RefreshColor();
        RefreshMesh(true);

        mIsVisible = false;
        gridRenderer.enabled = false;
    }

    void OnEnable() {
        if(refreshOnEnable) {
            RefreshColor();
            RefreshMesh(false);
        }

        if(hideOnEnable) {
            mIsVisible = false;
            gridRenderer.enabled = false;
        }
    }

    void OnDestroy() {
        if(mMat)
            Destroy(mMat);
    }

    void Awake() {
        mMat = gridRenderer.material;
        mIsVisible = gridRenderer.enabled;
    }
}
