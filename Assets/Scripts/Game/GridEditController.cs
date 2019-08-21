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
        Expand,
        Evaluate, //evaluate the goals by clustering cubes based on their data, then display each if they pass or fail
    }

    [Header("Data")]
    [SerializeField]
    GridLevelData _levelData = null;

    [SerializeField]
    [M8.TagSelector]
    string _tagEntityContainer = "";
    
    [SerializeField]
    [M8.TagSelector]
    string _tagGhostController = "";

    public GridLevelData levelData { get { return _levelData; } }

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

                //clear selection based on mode
                switch(mCurEditMode) {
                    case EditMode.None:
                    case EditMode.Evaluate:
                        mCurSelected = null;
                        break;
                }

                editChangedCallback?.Invoke();

                //run evaluation
                if(mCurEditMode == EditMode.Evaluate) {
                    StopCurRout();
                    mRout = StartCoroutine(DoGoalEvaluate());
                }
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

    public int GetAvailableCount() {
        var placedCount = 0;

        for(int i = 0; i < entityContainer.entities.Count; i++) {
            var ent = entityContainer.entities[i];

            //if we are in expand mode, ignore select's volume and use the volume from ghost
            if(editMode == EditMode.Expand && ent == selected)
                placedCount += ghostController.cellSize.volume;
            else
                placedCount += ent.cellSize.volume;
        }

        return levelData.resourceCount - placedCount;
    }

    public bool isBusy { get { return mRout != null; } }

    /// <summary>
    /// Called when mode and/or selection is changed
    /// </summary>
    public event System.Action editChangedCallback;

    private EditMode mCurEditMode = EditMode.None;    
    private GridEntity mCurSelected = null;

    private GridEntityContainer mEntityContainer;
    private GridGhostController mGhostController;

    private Coroutine mRout;

    protected override void OnInstanceDeinit() {
        GridEntityDisplay.ClearMeshCache();
        GridEntityDisplayFloor.ClearMeshCache();

        base.OnInstanceDeinit();
    }

    IEnumerator DoGoalEvaluate() {
        //group up entities
        var entGroups = entityContainer.GenerateEntityGroups();

        yield return null;

        mRout = null;
    }

    private void StopCurRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
}
