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

    [Header("Data")]
    public float topExpandDragScale = 1f;

    [Header("Display")]
    public GridGhostDisplay display;

    public GameObject faceHighlightTopGO;
    public GameObject faceHighlightFrontGO;
    public GameObject faceHighlightBackGO;
    public GameObject faceHighlightLeftGO;
    public GameObject faceHighlightRightGO;

    [Header("Signal Invoke")]
    public M8.Signal signalInvokeSizeChanged;
    public SignalGridEntity signalInvokeEntitySizeChanged;

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

                RefreshValid();
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

                RefreshValid();

                //update mesh
                if(mCellSize.isVolumeValid)
                    display.ApplyMesh(bounds, mCellSize);

                if(signalInvokeSizeChanged)
                    signalInvokeSizeChanged.Invoke();
            }
        }
    }

    public GridCell cellEnd {
        get {
            var _cellInd = cellIndex;
            var _cellSize = cellSize;

            return new GridCell { b = _cellInd.b + _cellSize.b - 1, row = _cellInd.row + _cellSize.row - 1, col = _cellInd.col + _cellSize.col - 1 };
        }
    }

    /// <summary>
    /// This is in local space
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

    /// <summary>
    /// Volume based on side measure from level data
    /// </summary>
    public float volume {
        get {
            var measure = GridEditController.instance.levelData.sideMeasure;
            var w = cellSize.col * measure;
            var l = cellSize.row * measure;
            var h = cellSize.b * measure;

            return w * h * l;
        }
    }

    public bool isDragging { get { return mDragFace != FaceFlags.None; } }

    public bool isValid { get; private set; }

    private GridController mController;

    private GridEntityData mData;

    private BoxCollider mColl;

    private GridCell mCellIndex = new GridCell { b = -1, row = -1, col = -1 };
    private GridCell mCellSize = new GridCell { b = 1, row = 1, col = 1 };

    private Mode mMode = Mode.Hidden;

    private FaceFlags mDragFace = FaceFlags.None;
    private GridCell mDragCellIndex;
    private GridCell mDragCellSizeStart; //our size when we first dragged

    private bool mIsFaceHighlightActive;

    void OnApplicationFocus(bool focus) {
        if(!focus)
            EndDrag();
    }

    void Awake() {
        mColl = GetComponent<BoxCollider>();

        RefreshMode();

        RefreshBounds();

        if(mCellSize.isVolumeValid)
            display.ApplyMesh(bounds, mCellSize);
    }

    void Update() {
        if(!isDragging && mIsFaceHighlightActive && mMode == Mode.Expand) {
            var eventSystem = EventSystem.current;
            if(eventSystem) {
                var faceHighlight = FaceFlags.None;

                var inputModule = eventSystem.currentInputModule as M8.UI.InputModule;
                if(inputModule) {
                    var eventData = inputModule.LastPointerEventData(-1);
                    if(eventData != null && eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject == gameObject)
                        faceHighlight = GetFaceFlag(eventData.pointerCurrentRaycast.worldNormal);
                }

                if(display.faceHighlight != faceHighlight) {
                    display.faceHighlight = faceHighlight;
                    RefreshHighlight();
                }
            }
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(mMode == Mode.None || mMode == Mode.Hidden || mMode == Mode.Move)
            return;

        mIsFaceHighlightActive = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(mMode == Mode.None || mMode == Mode.Hidden || mMode == Mode.Move)
            return;

        if(mIsFaceHighlightActive) {
            mIsFaceHighlightActive = false;

            if(!isDragging) {
                display.faceHighlight = FaceFlags.None;
                RefreshHighlight();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        //ensure there's a selection
        if(!GridEditController.instance.selected)
            return;

        //ensure it is valid
        var cast = eventData.pointerPressRaycast;
        if(cast.isValid && (cast.gameObject == gameObject || cast.gameObject == GridEditController.instance.selected.gameObject)) {
            mColl.enabled = false;

            mDragCellIndex.Invalidate();
            mDragCellSizeStart = cellSize;

            if(mode == Mode.Expand) {
                mDragFace = GetFaceFlag(eventData.pointerPressRaycast.worldNormal);
                display.faceHighlight = mDragFace;
                RefreshHighlight();
            }
            else
                mDragFace = FaceFlags.All;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        var editCtrl = GridEditController.instance;
        var curEnt = editCtrl.selected;

        //fail-safe, there should be a selection
        if(!curEnt) {
            EndDrag();
            return;
        }

        var cast = eventData.pointerCurrentRaycast;

        if(mode == Mode.Expand) {
            if(mDragFace == FaceFlags.Top) { //special case for top, rely on screen position delta
                var start = eventData.pressPosition;
                var end = eventData.position;

                var delta = end - start;

                var ind = Mathf.RoundToInt(delta.y * topExpandDragScale);

                var newSize = mDragCellSizeStart.b + ind;
                if(cellSize.b != newSize) {
                    if(newSize < 1)
                        newSize = 1;
                    else if(newSize > editCtrl.entityContainer.controller.cellSize.b)
                        newSize = editCtrl.entityContainer.controller.cellSize.b;

                    var _cellSize = cellSize;
                    _cellSize.b = newSize;
                    cellSize = _cellSize;

                    RefreshValid();

                    if(signalInvokeEntitySizeChanged)
                        signalInvokeEntitySizeChanged.Invoke(curEnt);
                }
            }
            else {
                //ensure collision is from level
                if(!cast.isValid)
                    return;

                if(cast.gameObject != editCtrl.entityContainer.gameObject)
                    return;

                var cell = editCtrl.entityContainer.controller.GetCell(cast.worldPosition, true);
                if(cell.isValid) {
                    if(mDragCellIndex != cell) {
                        mDragCellIndex = cell;

                        var _cellInd = cellIndex;
                        //var _cellSize = cellSize;
                        var _cellEnd = cellEnd;

                        //var _entCellInd = curEnt.cellIndex;
                        //var _entCellEnd = curEnt.cellEnd;

                        switch(mDragFace) {
                            case FaceFlags.Back:
                                if(cell.row <= _cellEnd.row)
                                    _cellInd.row = cell.row;
                                break;

                            case FaceFlags.Front:
                                if(cell.row >= _cellInd.row)
                                    _cellEnd.row = cell.row;
                                break;

                            case FaceFlags.Left:
                                if(cell.col <= _cellEnd.col)
                                    _cellInd.col = cell.col;
                                break;

                            case FaceFlags.Right:
                                if(cell.col >= _cellInd.col)
                                    _cellEnd.col = cell.col;
                                break;
                        }

                        cellIndex = _cellInd;
                        cellSize = new GridCell { b = _cellEnd.b - _cellInd.b + 1, row = _cellEnd.row - _cellInd.row + 1, col = _cellEnd.col - _cellInd.col + 1 };

                        RefreshValid();

                        if(signalInvokeEntitySizeChanged)
                            signalInvokeEntitySizeChanged.Invoke(curEnt);
                    }
                }
            }
        }
        else if(mode == Mode.Move) {
            //ensure collision is from level
            if(!cast.isValid)
                return;

            if(cast.gameObject != editCtrl.entityContainer.gameObject)
                return;

            var cell = editCtrl.entityContainer.controller.GetCell(cast.worldPosition, true);
            if(cell.isValid) {
                if(mDragCellIndex != cell) {
                    var apply = mDragCellIndex.isValid;

                    var deltaCell = new GridCell { b = 0, row = cell.row - mDragCellIndex.row, col = cell.col - mDragCellIndex.col };

                    mDragCellIndex = cell;

                    if(apply) {
                        var _cellIndex = new GridCell { b = cellIndex.b, row = cellIndex.row + deltaCell.row, col = cellIndex.col + deltaCell.col };
                        var _cellEnd = new GridCell { b = cellIndex.b, row = _cellIndex.row + cellSize.row - 1, col = _cellIndex.col + cellSize.col - 1 };

                        var gridCtrl = editCtrl.entityContainer.controller;

                        //clamp if out of bounds
                        if(_cellIndex.row < 0)
                            _cellIndex.row = 0;
                        else if(_cellEnd.row >= gridCtrl.cellSize.row)
                            _cellIndex.row = gridCtrl.cellSize.row - cellSize.row;

                        if(_cellIndex.col < 0)
                            _cellIndex.col = 0;
                        else if(_cellEnd.col >= gridCtrl.cellSize.col)
                            _cellIndex.col = gridCtrl.cellSize.col - cellSize.col;

                        if(gridCtrl.IsContained(_cellIndex))
                            cellIndex = _cellIndex;
                    }
                }
            }
            else
                mDragCellIndex.Invalidate();
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        EndDrag();
    }

    private FaceFlags GetFaceFlag(Vector3 worldNormal) {
        FaceFlags face = FaceFlags.None;

        var dir = transform.InverseTransformDirection(worldNormal);

        if(dir == Vector3.up)
            face = FaceFlags.Top;
        else if(dir == Vector3.forward)
            face = FaceFlags.Front;
        else if(dir == Vector3.back)
            face = FaceFlags.Back;
        else if(dir == Vector3.left)
            face = FaceFlags.Left;
        else if(dir == Vector3.right)
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

        if(mMode == Mode.Hidden) {
            display.faceHighlight = FaceFlags.None;
            display.isVisible = false;
        }
        else {
            display.isVisible = true;

            RefreshFaceHighlightFromMode();

            RefreshHighlight();

            RefreshValid();
        }

        mIsFaceHighlightActive = false;

        EndDrag();
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
            var pos = Vector3.zero;
            pos.y += cellSize.b * unitSize * 0.5f;
            bounds = new Bounds(pos, cellSize.GetSize(unitSize));
        }
        else
            bounds = new Bounds(Vector3.zero, Vector3.zero);

        //update collision
        if(bounds.size.x > 0f && bounds.size.y > 0f && bounds.size.z > 0f) {
            mColl.center = new Vector3(0f, bounds.extents.y, 0f);
            mColl.size = bounds.size;
        }

        //update face highlight positions
        faceHighlightTopGO.transform.localPosition = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        faceHighlightFrontGO.transform.localPosition = new Vector3(bounds.center.x, faceHighlightFrontGO.transform.localPosition.y, bounds.max.z);
        faceHighlightBackGO.transform.localPosition = new Vector3(bounds.center.x, faceHighlightBackGO.transform.localPosition.y, bounds.min.z);
        faceHighlightLeftGO.transform.localPosition = new Vector3(bounds.min.x, faceHighlightLeftGO.transform.localPosition.y, bounds.center.z);
        faceHighlightRightGO.transform.localPosition = new Vector3(bounds.max.x, faceHighlightRightGO.transform.localPosition.y, bounds.center.z);
    }

    private void RefreshValid() {
        var editCtrl = GridEditController.instance;
        var curEnt = editCtrl.selected;

        isValid = curEnt && editCtrl.entityContainer.IsPlaceable(cellIndex, cellSize, curEnt);
        if(isValid) {
            //check if available count is sufficient
            var count = editCtrl.GetAvailableCount();
            isValid = count >= 0;
        }

        display.SetPulseColorValid(isValid);
    }

    private void RefreshFaceHighlightFromMode() {
        switch(mMode) {
            case Mode.None:
            case Mode.Expand: //allow highlight to update
                display.faceHighlight = FaceFlags.None;
                break;
            case Mode.Move:
                display.faceHighlight = FaceFlags.All;
                break;
        }
    }

    private void EndDrag() {
        if(isDragging) {
            mDragFace = FaceFlags.None;
            RefreshFaceHighlightFromMode();
            RefreshHighlight();

            mColl.enabled = !(mMode == Mode.None || mMode == Mode.Hidden);
        }
    }
}
