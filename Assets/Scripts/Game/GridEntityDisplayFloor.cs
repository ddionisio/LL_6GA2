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
    private static Vector3[] mVtx = new Vector3[4];
    private static Vector2[] mUVs = new Vector2[4];

    private static Dictionary<GridCell, Mesh> mMeshCache = new Dictionary<GridCell, Mesh>();
    
    private int mGridRowCount = -1, mGridColCount = -1;

    private Material mMat;

    public static void ClearMeshCache() {
        foreach(var pair in mMeshCache) {
            if(pair.Value)
                Destroy(pair.Value);
        }

        mMeshCache.Clear();
    }

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

        var cellSize = gridEntity.cellSize;

        if(!forceRefresh && mGridRowCount == cellSize.row && mGridColCount == cellSize.col)
            return;

        mGridRowCount = cellSize.row;
        mGridColCount = cellSize.col;

        if(mGridRowCount <= 0 || mGridColCount <= 0)
            return;

        gridMeshFilter.sharedMesh = GenerateMesh(mGridRowCount, mGridColCount);
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
    }

    private static Mesh GenerateMesh(int row, int col) {
        Mesh mesh;

        var cellSize = new GridCell { b = 0, row = row, col = col };
        if(!mMeshCache.TryGetValue(cellSize, out mesh)) {
            var unitSize = GridEditController.instance.entityContainer.controller.unitSize;
            var pos = new Vector3(0f, cellSize.b * unitSize * 0.5f, 0f);
            var bounds = new Bounds(pos, cellSize.GetSize(unitSize));

            mVtx[0] = new Vector3(-bounds.extents.x, 0f, -bounds.extents.z);
            mVtx[1] = new Vector3(-bounds.extents.x, 0f, bounds.extents.z);
            mVtx[2] = new Vector3(bounds.extents.x, 0f, bounds.extents.z);
            mVtx[3] = new Vector3(bounds.extents.x, 0f, -bounds.extents.z);

            //var uvUnit = 1f;// textureTile / mUnitSize;

            var textureTile = GameData.instance.textureTile;
            var uvSize = new Vector2(textureTile * col, textureTile * row);

            mUVs[0] = new Vector2(0f, 0f);
            mUVs[1] = new Vector2(0f, uvSize.y);
            mUVs[2] = new Vector2(uvSize.x, uvSize.y);
            mUVs[3] = new Vector2(uvSize.x, 0f);

            mesh = new Mesh();
            mesh.vertices = mVtx;
            mesh.uv = mUVs;
            mesh.triangles = mInds;

            mMeshCache.Add(cellSize, mesh);
        }

        return mesh;
    }
}
