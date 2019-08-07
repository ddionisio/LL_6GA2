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

    public int volume { get { return b * row * col; } }

    public bool isValid { get { return row >= 0 && col >= 0 && b > 0; } }

    public void Invalidate() {
        row = col = b = -1;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
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

    public static bool operator ==(GridCell a, GridCell b) {
        return a.row == b.row && a.col == b.col && a.b == b.b;
    }

    public static bool operator !=(GridCell a, GridCell b) {
        return a.row != b.row || a.col != b.col || a.b != b.b;
    }
}
