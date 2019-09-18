using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity within GridEntityContainer. Ensure that this entity is inside the GridController hierarchy.
/// Assumes that pivot is bottom.
/// </summary>
[ExecuteInEditMode]
public class GridEntity : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public const string parmData = "data";
    public const string parmCellIndex = "cInd";
    public const string parmCellSize = "cSize";

    [SerializeField]
    GridEntityData _data = null;
    [SerializeField]
    GridCell _cellSize = new GridCell { b=1, row=1, col=1 };
    [SerializeField]
    bool _updateCellOnEnabled = false; //set to true for non-spawned entities

    [Header("Signal Invoke")]
    public SignalGridEntity signalInvokeEntitySizeChanged;

    public GridEntityData data { get { return _data; } }

    public GridEntityContainer container {
        get {
            if(!mContainer)
                mContainer = GetComponentInParent<GridEntityContainer>();
            return mContainer;
        }
    }

    public M8.PoolDataController poolDataController {
        get {
            if(!mPoolDataCtrl)
                mPoolDataCtrl = GetComponent<M8.PoolDataController>();
            return mPoolDataCtrl;
        }
    }

    /// <summary>
    /// Position within grid container (bottom-left)
    /// </summary>
    public GridCell cellIndex {
        get { return mCellIndex; }
        set {
            if(mCellIndex != value) {
                //do we need to move?
                if(mCellIndex.row != value.row || mCellIndex.col != value.col) {
                    mCellIndex = value;
                }
                else
                    mCellIndex = value;

                RefreshPosition();

                cellChangedCallback?.Invoke();
            }
        }
    }

    public GridCell cellSize {
        get { return _cellSize; }
        set {
            if(_cellSize != value) {
                //do we need to move?
                if(_cellSize.row != value.row || _cellSize.col != value.col) {
                    _cellSize = value;
                }
                else
                    _cellSize = value;

                mIsBoundsUpdated = false;

                cellChangedCallback?.Invoke();

                if(signalInvokeEntitySizeChanged)
                    signalInvokeEntitySizeChanged.Invoke(this);
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

    public Vector3 size {
        get {
            if(container) {
                var unitSize = container.controller.unitSize;
                return cellSize.GetSize(unitSize);
            }
            else
                return Vector3.zero;
        }
    }

    /// <summary>
    /// This is local
    /// </summary>
    public Bounds bounds {
        get {
            if(!mIsBoundsUpdated)
                RefreshBounds();

            return mBounds;
        }
    }

    public Vector3 anchorPosition {
        get {
            var pos = new Vector3(bounds.center.x, bounds.max.y + GameData.instance.anchorOffset, bounds.center.z);
            return transform.TransformPoint(pos);
        }
    }

    /// <summary>
    /// Volume based on side measure from level data
    /// </summary>
    public MixedNumber volume {
        get {
            var measure = GridEditController.instance.levelData.sideMeasure;
            var w = cellSize.col * measure;
            var l = cellSize.row * measure;
            var h = cellSize.b * measure;

            return w * h * l;
        }
    }

    public event System.Action cellChangedCallback;

    private GridCell mCellIndex = new GridCell { b=-1, col=-1, row=-1 }; //cell position bottom-left

    private M8.PoolDataController mPoolDataCtrl;
    private GridEntityContainer mContainer;

    private bool mIsCellUpdatedOnEnable;

    private Bounds mBounds;
    private bool mIsBoundsUpdated;

    public bool IsContained(GridCell index) {
        var _cellEnd = cellEnd;
        return index.row >= cellIndex.row && index.row <= _cellEnd.row && index.col >= cellIndex.col && index.col <= _cellEnd.col;
    }

    public void SetCell(GridCell index, GridCell size) {
        if(mCellIndex != index || _cellSize != size) {
            //do we need to move?
            var isMove = mCellIndex.col != index.col || mCellIndex.row != index.row;
            var isSizeChanged = _cellSize.col != size.col || _cellSize.row != size.row || _cellSize.b != size.b;
            if(isMove || isSizeChanged) {
                mCellIndex = index;
                _cellSize = size;

                RefreshPosition();

                mIsBoundsUpdated = false;

                cellChangedCallback?.Invoke();

                if(isSizeChanged && signalInvokeEntitySizeChanged)
                    signalInvokeEntitySizeChanged.Invoke(this);
            }
            else {
                mCellIndex = index;
                _cellSize = size;
            }
        }
    }

    /// <summary>
    /// This will update transform.position based on current cell info.
    /// </summary>
    public void RefreshPosition() {
        if(!container)
            return;

        var gridCtrl = container.controller;

        var size = cellSize.GetSize(gridCtrl.unitSize);

        var startBound = gridCtrl.GetBoundsFromCell(cellIndex);

        transform.localPosition = new Vector3(startBound.min.x + size.x * 0.5f, startBound.min.y, startBound.min.z + size.z * 0.5f);
    }

    /// <summary>
    /// This will update cell info based on transform.position, then snap position to grid.
    /// </summary>
    public void RefreshGridPostion() {
        if(!container)
            return;

        RefreshBounds();
        var b = bounds;

        /*//snap position
            var lpos = transform.localPosition + mBounds.min;

            var gridCtrl = container.controller;

            var cellPos = gridCtrl.GetCellLocal(lpos, false);
            var cellBound = gridCtrl.GetBoundsFromCell(cellPos);

            transform.localPosition =  new Vector3(cellBound.min.x + mBounds.extents.x, cellBound.min.y, cellBound.min.z + mBounds.extents.z);*/

        var gridCtrl = container.controller;

        //snap position
        var lpos = transform.localPosition + mBounds.min;

        var cellPos = gridCtrl.GetCellLocal(lpos, false);
        var cellBound = gridCtrl.GetBoundsFromCell(cellPos);

        transform.localPosition = new Vector3(cellBound.min.x + b.extents.x, cellBound.min.y, cellBound.min.z + b.extents.z);

        //apply cell index
        

        cellIndex = cellPos;
    }

    void OnDisable() {
        if(Application.isPlaying) {
            if(_updateCellOnEnabled) {
                container.RemoveEntity(this);
            }
        }
    }

    void OnEnable() {
        if(Application.isPlaying) {
            if(_updateCellOnEnabled) {
                RefreshBounds();
                RefreshGridPostion();

                container.AddEntity(this);
            }
        }
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        mCellIndex = new GridCell { b=0, row=0, col=0 };
        _cellSize = new GridCell { b=1, row=1, col=1 };


        if(parms != null) {
            if(parms.ContainsKey(parmData))
                _data = parms.GetValue<GridEntityData>(parmData);

            if(parms.ContainsKey(parmCellIndex))
                mCellIndex = parms.GetValue<GridCell>(parmCellIndex);

            if(parms.ContainsKey(parmCellSize))
                _cellSize = parms.GetValue<GridCell>(parmCellSize);
        }

        container.AddEntity(this);

        RefreshPosition();
        RefreshBounds();
    }

    void M8.IPoolDespawn.OnDespawned() {
        if(container)
            container.RemoveEntity(this);

        _data = null;
        mContainer = null;
    }

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            if(!container)
                return;

            RefreshBounds();

            //snap position
            var lpos = transform.localPosition + mBounds.min;

            var gridCtrl = container.controller;

            var cellPos = gridCtrl.GetCellLocal(lpos, false);
            var cellBound = gridCtrl.GetBoundsFromCell(cellPos);

            transform.localPosition =  new Vector3(cellBound.min.x + mBounds.extents.x, cellBound.min.y, cellBound.min.z + mBounds.extents.z);

            //apply cell index
            //var b = bounds;

            mCellIndex = cellPos;// gridCtrl.GetCellLocal(new Vector3(cellBound.center.x - b.extents.x, cellBound.min.y, cellBound.center.z - b.extents.z), false);
        }
    }
#endif

    private void RefreshBounds() {
        if(container) {
            var unitSize = container.controller.unitSize;
            var pos = new Vector3(0f, cellSize.b * unitSize * 0.5f, 0f);
            mBounds = new Bounds(pos, cellSize.GetSize(unitSize));
        }
        else
            mBounds = new Bounds(Vector3.zero, Vector3.zero);

        mIsBoundsUpdated = true;
    }

    void OnDrawGizmos() {
        if(cellSize.isValid && container) {
            Gizmos.color = Color.white;

            var b = bounds;

            //draw bounds
            M8.Gizmo.DrawWireCube(transform, b.center, b.extents);
        }
    }
}
