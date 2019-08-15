using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles edit state
/// </summary>
public class GridEditController : MonoBehaviour {
    public enum Mode {
        None,
        Select,
        Placement, //dragging block from palette
        Move,
        Expand
    }

    [Header("Data")]
    [SerializeField]
    GridController _controller = null;

    public GridController controller { get { return _controller; } }

    public Mode mode {
        get { return mCurMode; }
        set {
            if(mCurMode != value) {
                mCurMode = value;
                changedCallback?.Invoke();
            }
        }
    }

    public GridEntityEditController selected {
        get { return mCurSelected; }
        set {
            if(mCurSelected != value) {
                mCurSelected = value;
                changedCallback?.Invoke();
            }
        }
    }

    /// <summary>
    /// Called when mode and/or selection is changed
    /// </summary>
    public event System.Action changedCallback;

    private Mode mCurMode = Mode.None;
    private GridEntityEditController mCurSelected = null;
}
