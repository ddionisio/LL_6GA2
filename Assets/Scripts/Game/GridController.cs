using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {
    public const float boxColliderHeight = 0.1f;

    //NOTE: pivot is at bottom, cell sequence start at bottom-left

    [Header("Data")]
    public GridCell cellSize;
    public float unitSize = 0.5f;

    public Vector3 size { get { return new Vector3(cellSize.col * unitSize, cellSize.b * unitSize, cellSize.row * unitSize); } }

    public Vector3 extents {
        get {
            var s = unitSize * 0.5f;
            return new Vector3(cellSize.col * s, cellSize.b * s, cellSize.row * s);
        }
    }

    /// <summary>
    /// This is in local space
    /// </summary>
    public Bounds bounds {
        get {
            return new Bounds(new Vector3(0f, cellSize.b * unitSize * 0.5f, 0f), size);
        }
    }

    /// <summary>
    /// World space bottom-left corner
    /// </summary>
    public Vector3 min {
        get {
            var e = extents;
            return transform.TransformPoint(-e.x, 0f, -e.z);
        }
    }

    /// <summary>
    /// World space top-right corner
    /// </summary>
    public Vector3 max {
        get {
            var e = extents;
            return transform.TransformPoint(e.x, cellSize.b * unitSize, e.z);
        }
    }

    public BoxCollider boxCollider {
        get {
            if(!mIsBoxCollInit) {
                mBoxColl = GetComponent<BoxCollider>();
                mIsBoxCollInit = true;
            }

            return mBoxColl;
        }
    }

    private BoxCollider mBoxColl;
    private bool mIsBoxCollInit = false;

    public bool IsContained(GridCell index, GridCell size) {
        var volume = size.volume;

        if(volume > 0 && IsContained(index)) {
            if(volume > 1) {
                GridCell endIndex = new GridCell { b = index.b + size.b - 1, row = index.row + size.row - 1, col = index.col + size.col - 1 };
                if(IsContained(endIndex))
                    return true;
            }
            else
                return true;
        }

        return false;
    }

    public bool IsContained(GridCell index) {
        return index.isValid && index.b < cellSize.b && index.row < cellSize.row && index.col < cellSize.col;
    }

    public GridCell GetCell(Vector3 point, bool clamp) {
        var lpos = transform.InverseTransformPoint(point);
        return GetCellLocal(lpos, clamp);
    }

    public GridCell GetCellLocal(Vector3 localPoint, bool clamp) {
        var ext = extents;
        localPoint += new Vector3(ext.x, 0f, ext.z);

        var col = Mathf.FloorToInt(localPoint.x / unitSize);
        var row = Mathf.FloorToInt(localPoint.z / unitSize);
        var b = Mathf.FloorToInt(localPoint.y / unitSize);

        if(clamp)
            return new GridCell { col = Mathf.Clamp(col, 0, cellSize.col - 1), row = Mathf.Clamp(row, 0, cellSize.row - 1), b = Mathf.Clamp(b, 0, cellSize.b - 1) };
        else
            return new GridCell { col = col, row = row, b = b };
    }

    /// <summary>
    /// Return a bounds based on given cell (local space)
    /// </summary>
    public Bounds GetBoundsFromCell(GridCell cell) {
        var ext = extents;
        var hUnitSize = unitSize * 0.5f;

        var lpos = new Vector3((cell.col * unitSize - ext.x) + hUnitSize, cell.b * unitSize + hUnitSize, (cell.row * unitSize - ext.z) + hUnitSize);

        return new Bounds(lpos, new Vector3(unitSize, unitSize, unitSize));
    }

    void OnDrawGizmosSelected() {
        if(cellSize.isValid) {
            var b = bounds;

            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

            var y = b.min.y;

            //draw rows
            for(int r = 0; r <= cellSize.row; r++) {
                var z = b.min.z + r * unitSize;

                var pt1 = new Vector3(b.min.x, y, z);
                var pt2 = new Vector3(b.max.x, y, z);

                pt1 = transform.TransformPoint(pt1);
                pt2 = transform.TransformPoint(pt2);

                Gizmos.DrawLine(pt1, pt2);
            }

            //draw cols
            for(int c = 0; c <= cellSize.col; c++) {
                var x = b.min.x + c * unitSize;

                var pt1 = new Vector3(x, y, b.min.z);
                var pt2 = new Vector3(x, y, b.max.z);

                pt1 = transform.TransformPoint(pt1);
                pt2 = transform.TransformPoint(pt2);

                Gizmos.DrawLine(pt1, pt2);
            }
        }
    }

    void OnDrawGizmos() {
        if(cellSize.isValid) {            
            Gizmos.color = Color.white;

            var b = bounds;

            //draw bounds
            M8.Gizmo.DrawWireCube(transform, b.center, b.extents);
        }
    }
}
