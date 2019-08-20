using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles edit state
/// </summary>
public class GridEditController : GameModeController<GridEditController> {
    public enum EditMode {
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
    [M8.TagSelector]
    string _tagEntityContainer = "";
    
    [SerializeField]
    [M8.TagSelector]
    string _tagGhostController = "";

    public GridEntityDataGroup entityDataGroup { get { return _entityDataGroup; } }

    public GridEntityContainer entityContainer {
        get {
            if(!mEntityContainer) {
                var go = GameObject.FindGameObjectWithTag(_tagEntityContainer);
                mEntityContainer = go.GetComponent<GridEntityContainer>();
            }
            return mEntityContainer;
        }
    }

    public GridGhostController ghostController {
        get {
            if(!mGhostController) {
                var go = GameObject.FindGameObjectWithTag(_tagGhostController);
                mGhostController = go.GetComponent<GridGhostController>();
            }
            return mGhostController;
        }
    }

    public EditMode editMode {
        get { return mCurEditMode; }
        set {
            if(mCurEditMode != value) {
                mCurEditMode = value;
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
        var count = _entityDataGroup.GetCount(dat);
        var placedCount = entityContainer.GetVolumeCount(dat);

        //if we are in expand mode, use the volume from ghost
        if(editMode == EditMode.Expand) {
            if(selected && selected.data == dat) {
                placedCount -= selected.cellSize.volume;
                placedCount += ghostController.cellSize.volume;
            }
        }        

        return count - placedCount;
    }

    /// <summary>
    /// Called when mode and/or selection is changed
    /// </summary>
    public event System.Action editChangedCallback;

    private EditMode mCurEditMode = EditMode.None;    
    private GridEntity mCurSelected = null;

    private GridEntityContainer mEntityContainer;
    private GridGhostController mGhostController;
}
