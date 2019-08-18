using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use for display of resizing/moving. Ensure that this is inside the GridController hierarchy.
/// </summary>
public class GridGhostDisplay : MonoBehaviour {
    [Header("Display")]
    public MeshFilter cubeMeshFilter; //if not null, apply entity's dimension
    public Renderer rendererDisplay;

    [Header("Config")]
    public float textureTile = 1f;
    [SerializeField]
    float _alpha = 1f;
    [SerializeField]
    float _pulseScale = 0.5f;
    [SerializeField]
    float _baseAlpha = 0.5f;
    [SerializeField]
    Color _pulseColorInvalid = Color.red;

    public FaceFlags faceHighlight {
        get { return mFaceHighlight; }
        set {
            if(mFaceHighlight != value) {
                mFaceHighlight = value;

                //refresh color
                RefreshColorVertices();
            }
        }
    }

    public Material material { get; private set; }

    public Color pulseColor {
        get { return material ? material.GetColor(mShaderPulseColorId) : Color.clear; }
        set {
            if(material)
                material.SetColor(mShaderPulseColorId, value);
        }
    }

    public Color pulseColorDefault { get; private set; }

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

    //order: starting lower left, clockwise
    private Vector3[] mVtx;
    private Vector2[] mUVs;
    private Color32[] mClrs;

    private FaceFlags mFaceHighlight = FaceFlags.None;
        
    private Mesh mCubeMesh; //generated mesh if not available

    private int mShaderPulseColorId;

    /// <summary>
    /// Set pulse color based on valid. If true, set to default, otherwise invalid color.
    /// </summary>
    public void SetPulseColorValid(bool isValid) {
        pulseColor = isValid ? pulseColorDefault : _pulseColorInvalid;
    }

    public void ApplyMaterial(GridEntityData data) {
        var mat = data.material;

        mShaderPulseColorId = data.shaderPulseColorId;

        if(!material || !mat || material.name != mat.name) {
            if(material) {
                Destroy(material);
                material = null;
            }

            if(mat) {
                material = new Material(mat);
                rendererDisplay.sharedMaterial = material;

                //apply alpha
                var clr = material.GetColor(data.shaderColorId);
                clr.a = _alpha;
                material.SetColor(data.shaderColorId, clr);

                //apply pulse scale
                material.SetFloat(data.shaderPulseScaleId, _pulseScale);

                pulseColorDefault = material.GetColor(data.shaderPulseColorId);
            }
            else
                rendererDisplay.sharedMaterial = null;
        }
    }

    public void ApplyMesh(Bounds bounds, GridCell cellSize) {
        if(!cubeMeshFilter)
            return;

        bool isMeshInit = false;

        //ensure there is a mesh
        var mesh = cubeMeshFilter.sharedMesh;
        if(!mesh) {
            mCubeMesh = new Mesh();
            cubeMeshFilter.sharedMesh = mCubeMesh;

            isMeshInit = true;
        }

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

        if(isMeshInit)
            RefreshColorVertices();
    }

    private void RefreshColorVertices() {
        if(mClrs == null)
            mClrs = new Color32[vertexCount];

        var clr = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(255f * _baseAlpha));
        var clrHighlight = new Color32(255, 255, 255, 255);

        //top
        ApplyMeshColor(0, (mFaceHighlight & FaceFlags.Top) != FaceFlags.None ? clrHighlight : clr);

        //front
        ApplyMeshColor(4, (mFaceHighlight & FaceFlags.Front) != FaceFlags.None ? clrHighlight : clr);

        //back
        ApplyMeshColor(8, (mFaceHighlight & FaceFlags.Back) != FaceFlags.None ? clrHighlight : clr);

        //left
        ApplyMeshColor(12, (mFaceHighlight & FaceFlags.Left) != FaceFlags.None ? clrHighlight : clr);

        //right
        ApplyMeshColor(16, (mFaceHighlight & FaceFlags.Right) != FaceFlags.None ? clrHighlight : clr);

        if(cubeMeshFilter && cubeMeshFilter.sharedMesh)
            cubeMeshFilter.sharedMesh.colors32 = mClrs;
    }

    private void ApplyMeshData(int sInd, Vector3 vtx1, Vector3 vtx2, Vector3 vtx3, Vector3 vtx4, int tileRow, int tileCol) {
        mVtx[sInd] = vtx1;
        mVtx[sInd + 1] = vtx2;
        mVtx[sInd + 2] = vtx3;
        mVtx[sInd + 3] = vtx4;

        var uvSize = new Vector2(textureTile * tileCol, textureTile * tileRow);

        mUVs[sInd] = new Vector2(0f, 0f);
        mUVs[sInd + 1] = new Vector2(0f, uvSize.y);
        mUVs[sInd + 2] = new Vector2(uvSize.x, uvSize.y);
        mUVs[sInd + 3] = new Vector2(uvSize.x, 0f);
    }

    private void ApplyMeshColor(int sInd, Color clr) {
        mClrs[sInd] = clr;
        mClrs[sInd + 1] = clr;
        mClrs[sInd + 2] = clr;
        mClrs[sInd + 3] = clr;
    }

    void OnDestroy() {
        if(material)
            Destroy(material);

        if(mCubeMesh)
            Destroy(mCubeMesh);
    }

    void Awake() {
        //default hidden
        rendererDisplay.enabled = false;
    }
}