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
    public Text detailText;

    [Header("Anchor")]
    public RectTransform anchorClamp;

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

    private System.Text.StringBuilder mStrBuff = new System.Text.StringBuilder();

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
            ent.poolDataController.Release();

            GridEditController.instance.editMode = GridEditController.EditMode.Select;
            GridEditController.instance.selected = null;
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

            //clamp to offscreen anchor
            var ext = anchorClamp.sizeDelta * 0.5f;

            var anchorLocalPos = anchorClamp.InverseTransformPoint(anchorPos);
            
            if(anchorLocalPos.x < -ext.x)
                anchorLocalPos.x = -ext.x;
            else if(anchorLocalPos.x > ext.x)
                anchorLocalPos.x = ext.x;

            if(anchorLocalPos.y < -ext.y)
                anchorLocalPos.y = -ext.y;
            else if(anchorLocalPos.y > ext.y)
                anchorLocalPos.y = ext.y;

            anchorPos = anchorClamp.TransformPoint(anchorLocalPos);

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
                if(moveToggleGO) moveToggleGO.SetActive(moveActive);
                if(expandToggleGO) expandToggleGO.SetActive(expandActive);
            }
            else
                displayGO.SetActive(false);

            if(ghostMode != GridGhostController.Mode.Hidden && editCtrl.selected)
                ghost.data = editCtrl.selected.data;

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

        if(mCurEntity && mCurEntity.signalInvokeEntitySizeChanged) //this will refresh display on cards
            mCurEntity.signalInvokeEntitySizeChanged.Invoke(mCurEntity);
    }

    private void RefreshDimensionInfoDisplay() {
        GridCell size;
        MixedNumber volume;

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
                    volume = new MixedNumber();
                }
                break;
        }

        //generate dimension measurement
        var measureStr = UnitMeasure.GetText(editCtrl.levelData.measureType);

        mStrBuff.Clear();

        mStrBuff.AppendLine(size.ToString());

        MixedNumber num;

        num = size.col * editCtrl.levelData.sideMeasure; num.SimplifyImproper();
        mStrBuff.Append(num);
        mStrBuff.Append(measureStr);
        mStrBuff.Append(" x ");

        num = size.row * editCtrl.levelData.sideMeasure; num.SimplifyImproper();
        mStrBuff.Append(num);
        mStrBuff.Append(measureStr);
        mStrBuff.Append(" x ");

        num = size.b * editCtrl.levelData.sideMeasure; num.SimplifyImproper();
        mStrBuff.Append(num);
        mStrBuff.Append(measureStr);

        /*mStrBuff.Append('\n');

        mStrBuff.Append(UnitMeasure.GetVolumeText(editCtrl.levelData.measureType, volume));*/

        detailText.text = mStrBuff.ToString();
    }
}
