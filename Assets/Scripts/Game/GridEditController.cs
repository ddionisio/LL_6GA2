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
        Build
    }

    public struct EvaluateData {
        public GridEntityEditController[] entityEdits;
        public float volume;
        public float minHeight;
        public float maxHeight;

        public Bounds bounds; //local to level

        public GridEntityData data { get { return entityEdits != null && entityEdits.Length > 0 ? entityEdits[0].entity.data : null; } }

        public bool isValid { get { return entityEdits != null && entityEdits.Length > 0; } }

        public bool GoalIsVolumeMet(GridLevelData.Goal goal) {
            //TODO: conversion?
            return volume >= goal.volume;
        }

        public bool GoalIsHeightMet(GridLevelData.Goal goal) {
            return goal.heightRequire <= 0f || (minHeight == goal.heightRequire && minHeight == maxHeight);
        }

        public float GoalEfficiencyScale(GridLevelData.Goal goal) {
            var s = goal.volume / volume;
            return Mathf.Clamp01(s);
        }

        public EvaluateData(List<GridEntity> aEntities) {
            if(aEntities != null) {
                entityEdits = new GridEntityEditController[aEntities.Count];
                for(int i = 0; i < aEntities.Count; i++)
                    entityEdits[i] = aEntities[i].GetComponent<GridEntityEditController>();
            }
            else
                entityEdits = null;

            var sideVal = GridEditController.instance.levelData.sideMeasure;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            volume = 0f;
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;

            for(int i = 0; i < entityEdits.Length; i++) {
                var entEdit = entityEdits[i];

                volume += entEdit.entity.volume;

                var height = entEdit.entity.cellSize.b * sideVal;

                if(height < minHeight)
                    minHeight = height;
                if(height > maxHeight)
                    maxHeight = height;

                var b = entEdit.entity.bounds;

                var lpos = entEdit.transform.localPosition;
                var bMin = lpos + b.min;
                var bMax = lpos + b.max;

                if(bMin.x < min.x)
                    min.x = bMin.x;
                if(bMin.y < min.y)
                    min.y = bMin.y;
                if(bMin.z < min.z)
                    min.z = bMin.z;

                if(bMax.x > max.x)
                    max.x = bMax.x;
                if(bMax.y > max.y)
                    max.y = bMax.y;
                if(bMax.z > max.z)
                    max.z = bMax.z;
            }

            bounds = new Bounds();
            bounds.min = min;
            bounds.max = max;
        }

        public void SetPulseAlpha(float a) {
            if(!isValid) return;

            for(int i = 0; i < entityEdits.Length; i++)
                entityEdits[i].display.pulseScale = a;
        }
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

    [Header("Signal Invoke")]
    public M8.SignalVector3 signalInvokeCameraPanTo;

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

                StopCurRout();

                //generate specific data based on mode
                //clear selection based on mode
                switch(mCurEditMode) {
                    case EditMode.None:
                        mCurSelected = null;
                        break;

                    case EditMode.Evaluate:
                        mCurSelected = null;
                                                
                        GenerateEvaluation();

                        mCurrentEvaluateIndex = -1;
                        break;

                    case EditMode.Build:
                        mCurSelected = null;
                        break;
                }

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

    /// <summary>
    /// Generated when switching to evaluation, will have the same size as goals from level data
    /// </summary>
    public EvaluateData[] goalEvaluations { get; private set; }

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
    /// Used during evaluate mode
    /// </summary>
    public int currentEvaluateIndex {
        get { return mCurrentEvaluateIndex; }
        set {
            if(mCurrentEvaluateIndex != value) {
                EvaluateApplyFocus(value);

                evalCurrentChangedCallback?.Invoke();
            }
        }
    }

    public bool isAllGoalsMet {
        get {
            if(goalEvaluations == null)
                return false;

            for(int i = 0; i < goalEvaluations.Length; i++) {
                var eval = goalEvaluations[i];
                var goal = levelData.goals[i];

                if(!eval.isValid)
                    return false;

                if(!eval.GoalIsVolumeMet(goal))
                    return false;

                if(!eval.GoalIsHeightMet(goal))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Called when mode and/or selection is changed
    /// </summary>
    public event System.Action editChangedCallback;

    /// <summary>
    /// Called when current evaluation is changed
    /// </summary>
    public event System.Action evalCurrentChangedCallback;

    private EditMode mCurEditMode = EditMode.None;    
    private GridEntity mCurSelected = null;

    private GridEntityContainer mEntityContainer;
    private GridGhostController mGhostController;

    private int mCurrentEvaluateIndex;
        
    private Coroutine mRout;
        
    protected override void OnInstanceDeinit() {
        GridEntityDisplay.ClearMeshCache();
        GridEntityDisplayFloor.ClearMeshCache();

        base.OnInstanceDeinit();
    }

    private void GenerateEvaluation() {
        //group up entities
        var entGroups = entityContainer.GenerateEntityGroups();

        var evals = new M8.CacheList<EvaluateData>(entGroups.Count);

        for(int i = 0; i < entGroups.Count; i++) {
            var grp = entGroups[i];

            evals.Add(new EvaluateData(grp));
        }

        //filter based on goals
        goalEvaluations = new EvaluateData[levelData.goals.Length];

        for(int i = 0; i < levelData.goals.Length; i++) {
            var goal = levelData.goals[i];

            //grab most matching evaluation
            EvaluateData? dat = null;
            var datIsMet = false;
            var datVolumeDelta = float.MaxValue;

            for(int j = evals.Count - 1; j >= 0; j--) {
                var eval = evals[j];
                if(eval.data != goal.data)
                    continue;

                //check if met with goal
                var isMet = eval.GoalIsVolumeMet(goal) && eval.GoalIsHeightMet(goal);
                var delta = Mathf.Abs(eval.volume - goal.volume);

                if(!dat.HasValue || (isMet && !datIsMet)) {
                    dat = eval;
                    datVolumeDelta = delta;
                    datIsMet = true;

                    evals.RemoveAt(j);
                }
                else if(delta < datVolumeDelta) {
                    dat = eval;
                    datVolumeDelta = delta;

                    evals.RemoveAt(j);
                }
            }

            if(dat.HasValue)
                goalEvaluations[i] = dat.Value;
            else
                goalEvaluations[i] = new EvaluateData();
        }
    }

    private void EvaluateApplyFocus(int toIndex) {
        EvaluateData eval;

        if(mCurrentEvaluateIndex != -1) {
            //clear out focus from previous
            eval = goalEvaluations[mCurrentEvaluateIndex];
            eval.SetPulseAlpha(0f);
        }

        mCurrentEvaluateIndex = toIndex;

        //apply focus
        eval = goalEvaluations[mCurrentEvaluateIndex];
        if(eval.isValid) {
            eval.SetPulseAlpha(GameData.instance.selectHighlightPulseScale);

            if(signalInvokeCameraPanTo) {
                var b = eval.bounds;
                var focusPt = new Vector3(b.center.x, b.min.y, b.center.z);
                signalInvokeCameraPanTo.Invoke(focusPt);
            }
        }
    }

    private void StopCurRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
}
