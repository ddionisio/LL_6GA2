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
    [SerializeField]
    bool _refreshOnEnable = false; //set this to true for non-placeables

    public bool isVisible {
        get { return gridRenderer.enabled; }
        set {
            if(gridRenderer.enabled != value) {
                if(value)
                    RefreshMesh(false);

                gridRenderer.enabled = value;
            }
        }
    }

    private static readonly int[] mInds = new int[] { 0, 1, 2, 2, 3, 0 };

    //order: starting lower left, clockwise
    private Vector3[] mVtx = new Vector3[4];
    private Vector2[] mUVs = new Vector2[4];
    
    private int mGridRowCount = -1, mGridColCount = -1;

    private Material mMat;
    private Mesh mFloorMesh;

    public void RefreshColor() {
        if(!gridEntity || !gridEntity.data)
            return;

        if(!mMat)
            mMat = gridRenderer.material;

        var clr = gridEntity.data.color;
        clr.a = GameData.instance.floorAlpha;

        mMat.SetColor(gridEntity.data.shaderColorId, clr);
    }

    public void RefreshMesh(bool forceRefresh) {
        if(!gridEntity || !gridMeshFilter)
            return;

        //generate mesh if not set
        var mesh = gridMeshFilter.sharedMesh;
        if(!mesh) {
            mesh = mFloorMesh = new Mesh();
            gridMeshFilter.sharedMesh = mFloorMesh;
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
    }

    void OnEnable() {
        if(_refreshOnEnable) {
            RefreshColor();
            RefreshMesh(false);
        }
    }

    void OnDestroy() {
        if(mMat)
            Destroy(mMat);

        if(mFloorMesh)
            Destroy(mFloorMesh);
    }
}
