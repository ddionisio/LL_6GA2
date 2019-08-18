using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Use for resizing/moving controls. Ensure that this is inside the GridController hierarchy.
/// </summary>
public class GridGhostController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public enum Mode {
        Hidden,
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

    public Vector3 anchorPosition {
        get {
            var pos = new Vector3(bounds.center.x, bounds.max.y + GameData.instance.anchorOffset, bounds.center.z);
            return transform.TransformPoint(pos);
        }
    }

    public Mode mode {
        get { return mMode; }
        set {
            if(mMode != value) {
                mMode = value;
                RefreshMode();
            }
        }
    }

    public bool isDragging { get; private set; }

    private GridController mController;

    private GridEntityData mData;

    private BoxCollider mColl;

    private GridCell mCellIndex = new GridCell { b = -1, row = -1, col = -1 };
    private GridCell mCellSize = new GridCell { b = 1, row = 1, col = 1 };

    private Mode mMode = Mode.Hidden;

    private FaceFlags mPointerFace = FaceFlags.None;
    private FaceFlags mDragFace = FaceFlags.None;

    void Awake() {
        mColl = GetComponent<BoxCollider>();

        RefreshMode();

        RefreshBounds();

        if(mCellSize.isVolumeValid)
            display.ApplyMesh(bounds, mCellSize);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(mMode == Mode.None || mMode == Mode.Hidden)
            return;

        if(eventData.pointerCurrentRaycast.gameObject == gameObject)
            mPointerFace = GetFaceFlag(eventData.pointerCurrentRaycast.worldPosition);
        else
            mPointerFace = FaceFlags.None;

        if(!isDragging && mMode == Mode.Expand) {
            display.faceHighlight = mPointerFace;
            RefreshHighlight();
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(mMode == Mode.None || mMode == Mode.Hidden)
            return;

        mPointerFace = FaceFlags.None;
        if(!isDragging && mMode == Mode.Expand) {
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
        
    private void RefreshMode() {
        mColl.enabled = !(mMode == Mode.None || mMode == Mode.Hidden);

        if(mMode == Mode.Hidden)
            display.isVisible = false;
        else {
            switch(mMode) {
                case Mode.Hidden:
                case Mode.None:
                case Mode.Expand: //allow highlight to update
                    display.faceHighlight = FaceFlags.None;
                    break;
                case Mode.Move:
                    display.faceHighlight = FaceFlags.All;
                    break;
            }

            display.isVisible = true;
        }

        RefreshHighlight();
    }

    private void RefreshHighlight() {
        var face = display.faceHighlight;

        switch(mMode) {
            case Mode.None:
            case Mode.Hidden:
                faceHighlightTopGO.SetActive(false);
                faceHighlightFrontGO.SetActive(false);
                faceHighlightBackGO.SetActive(false);
                faceHighlightLeftGO.SetActive(false);
                faceHighlightRightGO.SetActive(false);
                break;

            case Mode.Expand:
                faceHighlightTopGO.SetActive((face & FaceFlags.Top) != FaceFlags.None);
                faceHighlightFrontGO.SetActive((face & FaceFlags.Front) != FaceFlags.None);
                faceHighlightBackGO.SetActive((face & FaceFlags.Back) != FaceFlags.None);
                faceHighlightLeftGO.SetActive((face & FaceFlags.Left) != FaceFlags.None);
                faceHighlightRightGO.SetActive((face & FaceFlags.Right) != FaceFlags.None);
                break;

            case Mode.Move:
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
