using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridEntityControlWidget : MonoBehaviour {
    [Header("Display")]
    public GameObject displayGO;
    public GameObject moveToggleGO;
    public GameObject expandToggleGO;
    public Text titleText;
    public Text gridDimensionText;

    [Header("Signal Listen")]
    public M8.Signal signalListenGhostSizeChanged;

    public Camera cam {
        get {
            if(!mCam)
                mCam = Camera.main;
            return mCam;
        }
    }

    private Camera mCam;

    private GridEditController.EditMode mCurEditMode = GridEditController.EditMode.None;
    private GridEntity mCurEntity;

    void OnDisable() {
        mCam = null;
        mCurEntity = null;

        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= OnEditChanged;

        if(signalListenGhostSizeChanged)
            signalListenGhostSizeChanged.callback -= RefreshDimensionInfoDisplay;
    }

    void OnEnable() {
        RefreshMode(true);

        GridEditController.instance.editChangedCallback += OnEditChanged;

        if(signalListenGhostSizeChanged)
            signalListenGhostSizeChanged.callback += RefreshDimensionInfoDisplay;
    }

    void Update() {
        if(mCurEntity) {
            Vector2 anchorPos;
            
            switch(mCurEditMode) {
                case GridEditController.EditMode.Select:
                    //anchor to entity
                    anchorPos = cam.WorldToScreenPoint(GridEditController.instance.selected.anchorPosition);
                    break;
                case GridEditController.EditMode.Move:
                case GridEditController.EditMode.Expand:
                    //anchor to ghost
                    anchorPos = cam.WorldToScreenPoint(GridEditController.instance.ghostController.anchorPosition);
                    break;
                default:
                    anchorPos = Vector2.zero;
                    break;
            }

            transform.position = anchorPos;
        }
    }

    void OnEditChanged() {
        RefreshMode(false);
    }

    private void RefreshMode(bool forceChanged) {
        var editCtrl = GridEditController.instance;
        var ghost = editCtrl.ghostController;

        //change state based on edit mode
        if(forceChanged || editCtrl.editMode != mCurEditMode || mCurEntity != editCtrl.selected) {
            mCurEditMode = editCtrl.editMode;

            var ghostMode = GridGhostController.Mode.Hidden;
            var displayVisible = false;
            var moveActive = false;
            var expandActive = false;

            switch(mCurEditMode) {
                case GridEditController.EditMode.Select:
                    displayVisible = editCtrl.selected != null;
                    break;

                case GridEditController.EditMode.Move:
                    moveActive = true;

                    if(editCtrl.selected) {
                        displayVisible = true;
                        ghostMode = GridGhostController.Mode.Move;
                    }
                    break;

                case GridEditController.EditMode.Expand:
                    expandActive = true;

                    if(editCtrl.selected) {
                        displayVisible = true;
                        ghostMode = GridGhostController.Mode.Expand;
                    }
                    break;
            }

            if(displayVisible) {
                displayGO.SetActive(true);
                moveToggleGO.SetActive(moveActive);
                expandToggleGO.SetActive(expandActive);
            }
            else
                displayGO.SetActive(false);

            ghost.mode = ghostMode;
        }

        //change info based on selected entity
        if(forceChanged || mCurEntity != editCtrl.selected) {
            mCurEntity = editCtrl.selected;
            if(mCurEntity) {
                displayGO.SetActive(true);

                //setup info
                titleText.text = M8.Localize.Get(mCurEntity.data.nameTextRef);

                RefreshDimensionInfoDisplay();

                //setup ghost selection
                switch(mCurEditMode) {
                    case GridEditController.EditMode.Expand:
                    case GridEditController.EditMode.Move:

                        //apply entity dimension to ghost
                        ghost.cellIndex = mCurEntity.cellIndex;
                        ghost.cellSize = mCurEntity.cellSize;
                        break;
                }
            }
        }
    }

    private void RefreshDimensionInfoDisplay() {
        GridCell size;

        var editCtrl = GridEditController.instance;
        switch(editCtrl.editMode) {
            case GridEditController.EditMode.Move:
            case GridEditController.EditMode.Expand:
                //use ghost info
                size = editCtrl.ghostController.cellSize;
                break;

            default:
                //use entity info
                if(mCurEntity)
                    size = mCurEntity.cellSize;
                else
                    size = new GridCell { b=0, row=0, col=0 };
                break;
        }

        gridDimensionText.text = size.ToString();
    }
}
