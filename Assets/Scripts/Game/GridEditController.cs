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
    GridEntityDataGroup _entityDataGroup = null;
    [SerializeField]
    GridEntityContainer _entityContainer = null;

    public GridEntityDataGroup entityDataGroup { get { return _entityDataGroup; } }

    public GridEntityContainer entityContainer { get { return _entityContainer; } }

    public Mode mode {
        get { return mCurMode; }
        set {
            if(mCurMode != value) {
                mCurMode = value;
                editChangedCallback?.Invoke();
            }
        }
    }

    public GridEntityEditController selected {
        get { return mCurSelected; }
        set {
            if(mCurSelected != value) {
                mCurSelected = value;
                editChangedCallback?.Invoke();
            }
        }
    }

    /// <summary>
    /// Called when mode and/or selection is changed
    /// </summary>
    public event System.Action editChangedCallback;

    private Mode mCurMode = Mode.None;    
    private GridEntityEditController mCurSelected = null;

}
