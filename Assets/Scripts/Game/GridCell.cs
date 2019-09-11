using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use for grid array [b, r, c]
/// </summary>
[System.Serializable]
public struct GridCell {
    /// <summary>
    /// Y-axis
    /// </summary>
    public int b;

    /// <summary>
    /// Z-axis
    /// </summary>
    public int row;

    /// <summary>
    /// X-axis
    /// </summary>
    public int col;

    public static GridCell zero { get { return new GridCell { b = 0, row = 0, col = 0 }; } }
    public static GridCell one { get { return new GridCell { b = 1, row = 1, col = 1 }; } }
    public static GridCell invalid { get { return new GridCell { b = -1, row = -1, col = -1 }; } }

    public int volume { get { return b * row * col; } }

    public bool isVolumeValid { get { return b > 0 && row > 0 && col > 0; } }

    public bool isValid { get { return row >= 0 && col >= 0 && b >= 0; } }

    public void Invalidate() {
        row = col = b = -1;
    }

    public Vector3 GetSize(float unitSize) {
        return new Vector3(col * unitSize, b * unitSize, row * unitSize);
    }

    public override string ToString() {
        return string.Format("{0} x {1} x {2}", col, row, b);
    }

    public override int GetHashCode() {
        return (row*100000 + col*100 + b).GetHashCode();
    }

    public override bool Equals(object obj) {
        if(obj is GridCell) {
            var other = (GridCell)obj;
            return row == other.row && col == other.col && b == other.b;
        }
        else
            return base.Equals(obj);
    }

    public int Compare(object x, object y) {
        return Compare((GridCell)x, (GridCell)y);
    }

    public int Compare(GridCell x, GridCell y) {
        if(x.b < y.b)
            return -1;
        else if(x.b > y.b)
            return 1;
        else if(x.b == y.b) {
            if(x.row < y.row)
                return -1;
            else if(x.row > y.row)
                return 1;
            else if(x.col != y.col)
                    return x.col < y.col ? -1 : 1;
        }

        return 0;
    }

    public static bool IsIntersectFloor(GridCell aIndex, GridCell aSize, GridCell bIndex, GridCell bSize) {
        var aIndexEnd = (aIndex + aSize) - 1;
        var bIndexEnd = (bIndex + bSize) - 1;

        //check a against b
        if(aIndex.row >= bIndex.row && aIndex.row <= bIndexEnd.row && aIndex.col >= bIndex.col && aIndex.col <= bIndexEnd.col)
            return true;
        else if(aIndexEnd.row >= bIndex.row && aIndexEnd.row <= bIndexEnd.row && aIndex.col >= bIndex.col && aIndex.col <= bIndexEnd.col)
            return true;
        else if(aIndexEnd.row >= bIndex.row && aIndexEnd.row <= bIndexEnd.row && aIndexEnd.col >= bIndex.col && aIndexEnd.col <= bIndexEnd.col)
            return true;
        else if(aIndex.row >= bIndex.row && aIndex.row <= bIndexEnd.row && aIndexEnd.col >= bIndex.col && aIndexEnd.col <= bIndexEnd.col)
            return true;

        //check b against a
        if(bIndex.row >= aIndex.row && bIndex.row <= aIndexEnd.row && bIndex.col >= aIndex.col && bIndex.col <= aIndexEnd.col)
            return true;
        else if(bIndexEnd.row >= aIndex.row && bIndexEnd.row <= aIndexEnd.row && bIndex.col >= aIndex.col && bIndex.col <= aIndexEnd.col)
            return true;
        else if(bIndexEnd.row >= aIndex.row && bIndexEnd.row <= aIndexEnd.row && bIndexEnd.col >= aIndex.col && bIndexEnd.col <= aIndexEnd.col)
            return true;
        else if(bIndex.row >= aIndex.row && bIndex.row <= aIndexEnd.row && bIndexEnd.col >= aIndex.col && bIndexEnd.col <= aIndexEnd.col)
            return true;

        return false;
    }

    public static bool operator ==(GridCell a, GridCell b) {
        return a.row == b.row && a.col == b.col && a.b == b.b;
    }

    public static bool operator !=(GridCell a, GridCell b) {
        return a.row != b.row || a.col != b.col || a.b != b.b;
    }

    public static GridCell operator +(GridCell a, GridCell b) {
        return new GridCell { b = a.b + b.b, row = a.row + b.row, col = a.col + b.col };
    }

    public static GridCell operator -(GridCell a, GridCell b) {
        return new GridCell { b = a.b - b.b, row = a.row - b.row, col = a.col - b.col };
    }

    public static GridCell operator -(GridCell a, int i) {
        return new GridCell { b = a.b - i, row = a.row - i, col = a.col - i };
    }
}
