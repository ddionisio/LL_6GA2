using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity within GridEntityContainer. Ensure that this entity is inside the GridController hierarchy.
/// Assumes that pivot is bottom.
/// </summary>
[ExecuteInEditMode]
public class GridEntity : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    [SerializeField]
    GridCell _cellSize;
    [SerializeField]
    bool _updateCellOnEnabled = false; //set to true for non-spawned entities

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

                    container.AddEntity(this);
                }
                else
                    mCellIndex = value;
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

                    container.AddEntity(this);
                }
                else
                    _cellSize = value;
            }
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
    /// This is local relative to container
    /// </summary>
    public Bounds bounds {
        get {
            if(container) {
                var unitSize = container.controller.unitSize;
                var pos = transform.localPosition;
                pos.y += cellSize.b * unitSize * 0.5f;
                return new Bounds(pos, cellSize.GetSize(unitSize));
            }
            else
                return new Bounds(transform.localPosition, Vector3.zero);
        }
    }

    GridCell mCellIndex = new GridCell { b=-1, col=-1, row=-1 }; //cell position bottom-left

    private M8.PoolDataController mPoolDataCtrl;
    private GridEntityContainer mContainer;

    private bool mIsCellUpdatedOnEnable;

    public void SetCell(GridCell index, GridCell size) {
        if(mCellIndex != index || _cellSize != size) {
            //do we need to move?
            if(mCellIndex.col != index.col || mCellIndex.row != index.row || _cellSize.col != size.col || _cellSize.row != size.row) {
                mCellIndex = index;
                _cellSize = size;

                container.AddEntity(this);
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

        var gridCtrl = container.controller;

        //snap position
        var lpos = transform.localPosition;

        var cellPos = gridCtrl.GetCellLocal(lpos, false);
        var cellBound = gridCtrl.GetBoundsFromCell(cellPos);

        transform.localPosition = new Vector3(cellBound.center.x, cellBound.min.y, cellBound.center.z);

        //apply cell index
        var b = bounds;

        cellIndex = gridCtrl.GetCellLocal(new Vector3(cellBound.center.x - b.extents.x, cellBound.min.y, cellBound.center.z - b.extents.z), false);
    }

    void OnEnable() {
        if(Application.isPlaying) {
            if(_updateCellOnEnabled)
                RefreshGridPostion();
        }
    }

#if UNITY_EDITOR
    void Update() {
        if(!Application.isPlaying) {
            if(!container)
                return;

            //snap position
            var lpos = transform.localPosition;

            var gridCtrl = container.controller;

            var cellPos = gridCtrl.GetCellLocal(lpos, false);
            var cellBound = gridCtrl.GetBoundsFromCell(cellPos);

            transform.localPosition = new Vector3(cellBound.center.x, cellBound.min.y, cellBound.center.z);

            //apply cell index
            var b = bounds;

            mCellIndex = gridCtrl.GetCellLocal(new Vector3(cellBound.center.x - b.extents.x, cellBound.min.y, cellBound.center.z - b.extents.z), false);
        }
    }
#endif

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        //set cell info if available
    }

    void M8.IPoolDespawn.OnDespawned() {
        if(container)
            container.RemoveEntity(this);
    }

    void OnDrawGizmos() {
        if(cellSize.isValid && container) {
            Gizmos.color = Color.white;

            var b = bounds;

            //draw bounds
            M8.Gizmo.DrawWireCube(container.transform, b.center, b.extents);
        }
    }
}
