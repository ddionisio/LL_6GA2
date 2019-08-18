using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles edit state
/// </summary>
public class GridEditController : M8.SingletonBehaviour<GridEditController> {
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
    [SerializeField]
    GridGhostController _ghostController = null;

    public GridEntityDataGroup entityDataGroup { get { return _entityDataGroup; } }

    public GridEntityContainer entityContainer { get { return _entityContainer; } }

    public GridGhostController ghostController { get { return _ghostController; } }

    public Mode mode {
        get { return mCurMode; }
        set {
            if(mCurMode != value) {
                mCurMode = value;
                editChangedCallback?.Invoke();
            }
        }
    }

    public GridEntity selected {
        get { return mCurSelected; }
        set {
            if(mCurSelected != value) {
                mCurSelected = value;
                editChangedCallback?.Invoke();
            }
        }
    }

    public int GetAvailableCount(GridEntityData dat) {
        var placedCount = entityContainer.GetVolumeCount(dat);
        var count = _entityDataGroup.GetCount(dat);

        return count - placedCount;
    }

    /// <summary>
    /// Called when mode and/or selection is changed
    /// </summary>
    public event System.Action editChangedCallback;

    private Mode mCurMode = Mode.None;    
    private GridEntity mCurSelected = null;

}
