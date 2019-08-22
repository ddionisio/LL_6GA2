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
    public Text volumeText;

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

    public void ExpandToggle() {
        var editCtrl = GridEditController.instance;

        if(editCtrl.editMode == GridEditController.EditMode.Expand) {
            ToSelect();
        }
        else
            editCtrl.editMode = GridEditController.EditMode.Expand;
    }

    public void MoveToggle() {
        var editCtrl = GridEditController.instance;

        if(editCtrl.editMode == GridEditController.EditMode.Move) {
            ToSelect();
        }
        else
            editCtrl.editMode = GridEditController.EditMode.Move;
    }

    public void ToSelect() {
        var editCtrl = GridEditController.instance;

        if(mCurEntity) {
            //commit ghost if valid
            if(editCtrl.ghostController.isValid) {
                var toCellIndex = editCtrl.ghostController.cellIndex;
                var toCellSize = editCtrl.ghostController.cellSize;

                editCtrl.editMode = GridEditController.EditMode.Select;

                mCurEntity.SetCell(toCellIndex, toCellSize);
            }
            else {
                editCtrl.editMode = GridEditController.EditMode.Select;

                if(mCurEntity.signalInvokeEntitySizeChanged) //this will refresh display on cards
                    mCurEntity.signalInvokeEntitySizeChanged.Invoke(mCurEntity);
            }
        }
        else //fail-safe
            editCtrl.editMode = GridEditController.EditMode.Select;
    }

    public void Delete() {
        if(mCurEntity && mCurEntity.poolDataController) {
            var ent = mCurEntity;

            GridEditController.instance.editMode = GridEditController.EditMode.Select;
            GridEditController.instance.selected = null;
                        
            ent.poolDataController.Release();
        }
    }

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

        var prevEditMode = mCurEditMode;

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
            }
        }

        switch(prevEditMode) {
            case GridEditController.EditMode.Placement:
            case GridEditController.EditMode.Select:
            case GridEditController.EditMode.None:
                if(ghost.mode != GridGhostController.Mode.Hidden && mCurEntity) {
                    ghost.cellIndex = mCurEntity.cellIndex;
                    ghost.cellSize = mCurEntity.cellSize;
                }
                break;
        }
    }

    private void RefreshDimensionInfoDisplay() {
        GridCell size;
        float volume;

        var editCtrl = GridEditController.instance;
        switch(editCtrl.editMode) {
            case GridEditController.EditMode.Move:
            case GridEditController.EditMode.Expand:
                //use ghost info
                size = editCtrl.ghostController.cellSize;
                volume = editCtrl.ghostController.volume;
                break;

            default:
                //use entity info
                if(mCurEntity) {
                    size = mCurEntity.cellSize;
                    volume = mCurEntity.volume;
                }
                else {
                    size = GridCell.zero;
                    volume = 0f;
                }
                break;
        }

        gridDimensionText.text = size.ToString();
        volumeText.text = UnitMeasure.GetVolumeText(editCtrl.levelData.measureType, volume);
    }
}
