using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityDisplay : MonoBehaviour, M8.IPoolSpawnComplete {
    [Header("Data")]
    public GridEntity gridEntity;

    [Header("Display")]
    public MeshFilter cubeMeshFilter; //if not null, apply entity's dimension
    public Renderer rendererDisplay; //use for fade/pulse
        
    [Header("Config")]
    public float textureTile = 1f;

    public float alpha {
        get { return mAlpha; }
        set {
            if(mAlpha != value) {
                mAlpha = value;
                ApplyAlpha();
            }
        }
    }

    public float pulseScale {
        get { return mPulseScale; }
        set {
            if(mPulseScale != value) {
                mPulseScale = value;
                ApplyPulseScale();
            }
        }
    }

    public Material material {
        get {
            if(!mMat)
                mMat = rendererDisplay.material;

            return mMat;
        }
    }

    public bool isVisible {
        get { return rendererDisplay.enabled; }
        set { rendererDisplay.enabled = value; }
    }

    private static readonly int[] mInds = new int[] {
         0,  1,  2,  2,  3,  0, //top
         4,  5,  6,  6,  7,  4, //back
         8,  9, 10, 10, 11,  8, //front
        12, 13, 14, 14, 15, 12, //left
        16, 17, 18, 18, 19, 16, //right
    };

    private const int vertexCount = 20;

    private static Color32[] vertexWhiteColors {
        get {
            if(mWhiteClrs == null) {
                mWhiteClrs = new Color32[vertexCount];
                for(int i = 0; i < mWhiteClrs.Length; i++)
                    mWhiteClrs[i] = new Color32(255, 255, 255, 255);
            }
            return mWhiteClrs;
        }
    }
    private static Color32[] mWhiteClrs;

    //order: starting lower left, clockwise
    private Vector3[] mVtx;
    private Vector2[] mUVs;

    private GridCell mCellSize = new GridCell { b = -1, row = -1, col = -1 };

    private Mesh mCubeMesh; //generated mesh if not available

    private Material mMat;
    private float mAlpha;
    private float mPulseScale;

    public void ApplyMaterial(Material mat) {
        if(rendererDisplay.sharedMaterial == null || mat == null || rendererDisplay.sharedMaterial.name != mat.name) {
            if(mMat) {
                Destroy(mMat);
                mMat = null;
            }

            rendererDisplay.sharedMaterial = mat;
        }

        //apply current properties, this will also initialize mMat
        ApplyAlpha();
        ApplyPulseScale();
    }

    public void RefreshMesh(bool forceRefresh) {
        if(!gridEntity || !cubeMeshFilter)
            return;
                
        var cellSize = gridEntity.cellSize;

        if(!forceRefresh && mCellSize == cellSize)
            return;

        mCellSize = cellSize;

        //generate mesh first-time
        var mesh = cubeMeshFilter.sharedMesh;
        if(!mesh) {
            mesh = mCubeMesh = new Mesh();
            cubeMeshFilter.sharedMesh = mCubeMesh;
        }
        else if(mesh.vertexCount != vertexCount)
            mesh.Clear();

        var bounds = gridEntity.bounds;

        if(mVtx == null)
            mVtx = new Vector3[vertexCount];
        if(mUVs == null)
            mUVs = new Vector2[vertexCount];

        //top
        ApplyMeshData(0,
            new Vector3(-bounds.extents.x, bounds.size.y, -bounds.extents.z),
            new Vector3(-bounds.extents.x, bounds.size.y, bounds.extents.z),
            new Vector3(bounds.extents.x, bounds.size.y, bounds.extents.z),
            new Vector3(bounds.extents.x, bounds.size.y, -bounds.extents.z),
            cellSize.row, cellSize.col);

        //front
        ApplyMeshData(4,
            new Vector3(bounds.extents.x, 0f, bounds.extents.z),
            new Vector3(bounds.extents.x, bounds.size.y, bounds.extents.z),
            new Vector3(-bounds.extents.x, bounds.size.y, bounds.extents.z),
            new Vector3(-bounds.extents.x, 0f, bounds.extents.z),
            cellSize.b, cellSize.col);

        //back
        ApplyMeshData(8,
            new Vector3(-bounds.extents.x, 0f, -bounds.extents.z),
            new Vector3(-bounds.extents.x, bounds.size.y, -bounds.extents.z),
            new Vector3(bounds.extents.x, bounds.size.y, -bounds.extents.z),
            new Vector3(bounds.extents.x, 0f, -bounds.extents.z),
            cellSize.b, cellSize.col);

        //left
        ApplyMeshData(12,
            new Vector3(-bounds.extents.x, 0f, bounds.extents.z),
            new Vector3(-bounds.extents.x, bounds.size.y, bounds.extents.z),
            new Vector3(-bounds.extents.x, bounds.size.y, -bounds.extents.z),
            new Vector3(-bounds.extents.x, 0f, -bounds.extents.z),
            cellSize.b, cellSize.row);

        //right
        ApplyMeshData(16,
            new Vector3(bounds.extents.x, 0f, -bounds.extents.z),
            new Vector3(bounds.extents.x, bounds.size.y, -bounds.extents.z),
            new Vector3(bounds.extents.x, bounds.size.y, bounds.extents.z),
            new Vector3(bounds.extents.x, 0f, bounds.extents.z),
            cellSize.b, cellSize.row);

        mesh.vertices = mVtx;
        mesh.uv = mUVs;
        mesh.triangles = mInds;
        mesh.colors32 = vertexWhiteColors;
    }

    private void ApplyMeshData(int sInd, Vector3 vtx1, Vector3 vtx2, Vector3 vtx3, Vector3 vtx4, int tileRow, int tileCol) {
        mVtx[sInd]     = vtx1;
        mVtx[sInd + 1] = vtx2;
        mVtx[sInd + 2] = vtx3;
        mVtx[sInd + 3] = vtx4;

        var uvSize = new Vector2(textureTile * tileCol, textureTile * tileRow);

        mUVs[sInd]     = new Vector2(0f, 0f);
        mUVs[sInd + 1] = new Vector2(0f, uvSize.y);
        mUVs[sInd + 2] = new Vector2(uvSize.x, uvSize.y);
        mUVs[sInd + 3] = new Vector2(uvSize.x, 0f);
    }

    void M8.IPoolSpawnComplete.OnSpawnComplete() {
        var dat = gridEntity ? gridEntity.data : null;
        if(dat)
            ApplyMaterial(gridEntity.data.material);

        RefreshMesh(true);
    }

    void OnDestroy() {
        if(gridEntity)
            gridEntity.cellChangedCallback -= OnGridEntityCellChanged;

        if(mMat)
            Destroy(mMat);

        if(mCubeMesh)
            Destroy(mCubeMesh);
    }

    void Awake() {
        //initialize property values
        var dat = gridEntity ? gridEntity.data : null;
        var mat = rendererDisplay.sharedMaterial;

        if(dat && mat) {
            if(mat.HasProperty(dat.shaderColorId)) {
                var clr = mat.GetColor(dat.shaderColorId);
                mAlpha = clr.a;
            }

            if(mat.HasProperty(dat.shaderPulseScaleId))
                mPulseScale = mat.GetFloat(dat.shaderPulseScaleId);
        }

        if(gridEntity)
            gridEntity.cellChangedCallback += OnGridEntityCellChanged;
    }

    void OnGridEntityCellChanged() {
        RefreshMesh(false);
    }

    private void ApplyAlpha() {
        var dat = gridEntity ? gridEntity.data : null;
        var mat = material;
        if(dat && mat && mat.HasProperty(dat.shaderColorId)) {
            var clr = mat.GetColor(dat.shaderColorId);
            clr.a = mAlpha;
            mat.SetColor(dat.shaderColorId, clr);
        }
    }

    private void ApplyPulseScale() {
        var dat = gridEntity ? gridEntity.data : null;
        var mat = material;
        if(dat && mat)
            mat.SetFloat(dat.shaderPulseScaleId, mPulseScale);
    }
}