using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridControllerDisplay : MonoBehaviour {
    [Header("Data")]
    public GridController gridControl;
    
    [Header("Display")]
    public MeshFilter gridMeshFilter;
    public Renderer rendererDisplay;

    [Header("Config")]
    public float textureTile = 1f;
    public bool refreshOnEnable = true;
    public bool hideOnEnable = true;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeShow;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeHide;

    public bool isVisible {
        get { return mIsVisible; }
        set {
            if(mIsVisible != value) {
                mIsVisible = value;

                if(mIsVisible) {
                    rendererDisplay.enabled = true;

                    if(animator && !string.IsNullOrEmpty(takeShow))
                        animator.Play(takeShow);
                }
                else {
                    if(animator && !string.IsNullOrEmpty(takeHide))
                        animator.Play(takeHide);
                }
            }
        }
    }

    private static readonly int[] mInds = new int[] { 0, 1, 2, 2, 3, 0 };

    //order: starting lower left, clockwise
    private Vector3[] mVtx = new Vector3[4];
    private Vector2[] mUVs = new Vector2[4];

    private int mGridRowCount = -1, mGridColCount = -1;
    private float mUnitSize = 0f;

    private bool mIsVisible;

    public void RefreshMesh(bool forceRefresh) {
        if(!gridControl || !gridMeshFilter)
            return;

        //generate mesh if not set
        var mesh = gridMeshFilter.sharedMesh;
        if(!mesh) {
            mesh = new Mesh();
            gridMeshFilter.sharedMesh = mesh;
        }

        var cellSize = gridControl.cellSize;

        if(!forceRefresh && mGridRowCount == cellSize.row && mGridColCount == cellSize.col && mUnitSize == gridControl.unitSize)
            return;
                
        mGridRowCount = cellSize.row;
        mGridColCount = cellSize.col;
        mUnitSize = gridControl.unitSize;

        if(mUnitSize <= 0f || mGridRowCount <= 0 || mGridColCount <= 0)
            return;

        var bounds = gridControl.bounds;

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

    void OnEnable() {
        if(refreshOnEnable)
            RefreshMesh(false);

        if(hideOnEnable) {
            mIsVisible = false;
            rendererDisplay.enabled = false;
        }
    }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorTakeEnd;
    }

    void Awake() {
        if(animator)
            animator.takeCompleteCallback += OnAnimatorTakeEnd;

        mIsVisible = rendererDisplay.enabled;
    }

    void OnAnimatorTakeEnd(M8.Animator.Animate anim, M8.Animator.Take take) {
        if(take.name == takeHide) {
            rendererDisplay.enabled = false;
        }
    }
}
