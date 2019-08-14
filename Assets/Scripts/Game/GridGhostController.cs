using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Use for resizing/moving controls. Ensure that this is inside the GridController hierarchy.
/// </summary>
public class GridGhostController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public enum InputMode {
        None,
        Move,
        Expand
    }

    [Header("Display")]
    public GridGhostDisplay display;

    public GameObject faceHighlightTopGO;
    public GameObject faceHighlightFrontGO;
    public GameObject faceHighlightBackGO;
    public GameObject faceHighlightLeftGO;
    public GameObject faceHighlightRightGO;

    public GridEntityData data {
        get { return mData; }
        set {
            if(mData != value) {
                mData = value;

                //update material
                if(mData)
                    display.ApplyMaterial(mData);
            }
        }
    }

    public GridController controller {
        get {
            if(!mController) {
                mController = GetComponentInParent<GridController>();

                RefreshPosition();
            }

            return mController;
        }
    }

    public GridCell cellIndex {
        get { return mCellIndex; }
        set {
            if(mCellIndex != value) {
                mCellIndex = value;

                //update position
                RefreshPosition();

                RefreshVisible();
            }
        }
    }

    public GridCell cellSize {
        get { return mCellSize; }
        set {
            if(mCellSize != value) {
                //do we need to move?
                if(mCellSize.row != value.row || mCellSize.col != value.col) {
                    mCellSize = value;

                    RefreshPosition();
                }
                else
                    mCellSize = value;

                RefreshVisible();

                //update bounds
                RefreshBounds();

                //update mesh
                if(mCellSize.isVolumeValid)
                    display.ApplyMesh(bounds, mCellSize);
            }
        }
    }

    /// <summary>
    /// This is local relative to container
    /// </summary>
    public Bounds bounds { get; private set; }

    public bool isVisible {
        get { return mIsVisible; }
        set {
            if(mIsVisible != value) {
                mIsVisible = value;
                RefreshVisible();
            }
        }
    }

    public InputMode inputMode {
        get { return mInputMode; }
        set {
            if(mInputMode != value) {
                mInputMode = value;
                RefreshInputMode();
            }
        }
    }

    public bool isDragging { get; private set; }

    private GridController mController;

    private GridEntityData mData;

    private BoxCollider mColl;

    private GridCell mCellIndex = new GridCell { b = -1, row = -1, col = -1 };
    private GridCell mCellSize = new GridCell { b = 1, row = 1, col = 1 };

    private bool mIsVisible = false;
    private InputMode mInputMode = InputMode.None;

    private FaceFlags mPointerFace = FaceFlags.None;
    private FaceFlags mDragFace = FaceFlags.None;

    void Awake() {
        mColl = GetComponent<BoxCollider>();
                
        RefreshVisible();

        RefreshInputMode();

        RefreshBounds();

        if(mCellSize.isVolumeValid)
            display.ApplyMesh(bounds, mCellSize);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(mInputMode == InputMode.None)
            return;

        if(eventData.pointerCurrentRaycast.gameObject == gameObject)
            mPointerFace = GetFaceFlag(eventData.pointerCurrentRaycast.worldPosition);
        else
            mPointerFace = FaceFlags.None;

        if(!isDragging && mInputMode == InputMode.Expand) {
            display.faceHighlight = mPointerFace;
            RefreshHighlight();
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(mInputMode == InputMode.None)
            return;

        mPointerFace = FaceFlags.None;
        if(!isDragging && mInputMode == InputMode.Expand) {
            display.faceHighlight = mPointerFace;
            RefreshHighlight();
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {

    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!isDragging)
            return;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!isDragging)
            return;
    }

    private FaceFlags GetFaceFlag(Vector3 worldPos) {
        FaceFlags face = FaceFlags.None;

        var lpos = transform.InverseTransformPoint(worldPos);

        var dpos = lpos - bounds.center;

        var dir = dpos.normalized;

        if(Vector3.Angle(dir, Vector3.up) <= 45f)
            face = FaceFlags.Top;
        else if(Vector3.Angle(dir, Vector3.forward) <= 45f)
            face = FaceFlags.Front;
        else if(Vector3.Angle(dir, Vector3.back) <= 45f)
            face = FaceFlags.Back;
        else if(Vector3.Angle(dir, Vector3.left) <= 45f)
            face = FaceFlags.Left;
        else if(Vector3.Angle(dir, Vector3.right) <= 45f)
            face = FaceFlags.Right;

        return face;
    }

    private void RefreshPosition() {
        var ctrl = controller;
        if(!ctrl)
            return;

        var size = cellSize.GetSize(ctrl.unitSize);

        var startBound = ctrl.GetBoundsFromCell(cellIndex);

        transform.localPosition = new Vector3(startBound.min.x + size.x * 0.5f, startBound.min.y, startBound.min.z + size.z * 0.5f);
    }

    private void RefreshVisible() {
        display.isVisible = mIsVisible && mCellSize.isVolumeValid && controller.IsContained(mCellIndex);
    }
        
    private void RefreshInputMode() {
        switch(mInputMode) {
            case InputMode.None:
            case InputMode.Expand: //allow highlight to update
                display.faceHighlight = FaceFlags.None;
                break;
            case InputMode.Move:
                display.faceHighlight = FaceFlags.All;
                break;
        }

        mColl.enabled = mInputMode != InputMode.None;

        RefreshHighlight();
    }

    private void RefreshHighlight() {
        var face = display.faceHighlight;

        switch(mInputMode) {
            case InputMode.None:
                faceHighlightTopGO.SetActive(false);
                faceHighlightFrontGO.SetActive(false);
                faceHighlightBackGO.SetActive(false);
                faceHighlightLeftGO.SetActive(false);
                faceHighlightRightGO.SetActive(false);
                break;

            case InputMode.Expand:
                faceHighlightTopGO.SetActive((face & FaceFlags.Top) != FaceFlags.None);
                faceHighlightFrontGO.SetActive((face & FaceFlags.Front) != FaceFlags.None);
                faceHighlightBackGO.SetActive((face & FaceFlags.Back) != FaceFlags.None);
                faceHighlightLeftGO.SetActive((face & FaceFlags.Left) != FaceFlags.None);
                faceHighlightRightGO.SetActive((face & FaceFlags.Right) != FaceFlags.None);
                break;

            case InputMode.Move:
                faceHighlightTopGO.SetActive(false);
                faceHighlightFrontGO.SetActive(true);
                faceHighlightBackGO.SetActive(true);
                faceHighlightLeftGO.SetActive(true);
                faceHighlightRightGO.SetActive(true);
                break;
        }
    }

    private void RefreshBounds() {
        if(controller) {
            var unitSize = controller.unitSize;
            var pos = transform.localPosition;
            pos.y += cellSize.b * unitSize * 0.5f;
            bounds = new Bounds(pos, cellSize.GetSize(unitSize));
        }
        else
            bounds = new Bounds(transform.localPosition, Vector3.zero);

        //update collision
        if(bounds.size.x > 0f && bounds.size.y > 0f && bounds.size.z > 0f) {
            mColl.center = new Vector3(0f, bounds.extents.y, 0f);
            mColl.size = bounds.size;
        }

        //update face highlight positions
        faceHighlightTopGO.transform.localPosition = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        faceHighlightFrontGO.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, bounds.max.z);
        faceHighlightBackGO.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z);
        faceHighlightLeftGO.transform.localPosition = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
        faceHighlightRightGO.transform.localPosition = new Vector3(bounds.max.x, bounds.center.y, bounds.center.z);
    }
}
