using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Check GridEditController state to change visibility, do highlight during edit mode, update collider based on entity
/// </summary>
public class GridEntityEditController : MonoBehaviour, M8.IPoolSpawnComplete, M8.IPoolDespawn, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [Header("Data")]
    public GridEntity entity;
    public GridEntityDisplay display;
    public GridEntityDisplayFloor displayFloor;

    [Header("Config")]
    [SerializeField]
    bool _isNonPlaceable = false; //set this to true for non-placeables

    public BoxCollider collision {
        get {
            if(!mColl)
                mColl = GetComponent<BoxCollider>();
            return mColl;
        }
    }

    public GridEditController editController {
        get {
            if(!mEditCtrl)
                mEditCtrl = GetComponentInParent<GridEditController>();
            return mEditCtrl;
        }
    }

    public bool isSelected { get { return editController && editController.selected == this; } }
    public GridEditController.Mode mode { get { return editController ? editController.mode : GridEditController.Mode.None; } }

    public Vector3 anchorPosition {
        get {
            var bounds = entity.bounds;
            var pos = new Vector3(bounds.center.x, bounds.max.y + GameData.instance.anchorOffset, bounds.center.z);
            return transform.TransformPoint(pos);
        }
    }

    private BoxCollider mColl;
    private GridEditController mEditCtrl;

    private bool mIsHighlighted;

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            mIsHighlighted = false;
            RefreshHighlight();
        }
    }

    void OnDisable() {
        if(_isNonPlaceable) {
            Deinit();
        }
    }

    void OnEnable() {
        if(_isNonPlaceable) {
            Init();
        }
    }

    void M8.IPoolDespawn.OnDespawned() {
        Deinit();
    }

    void M8.IPoolSpawnComplete.OnSpawnComplete() {
        Init();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(_isNonPlaceable || mode != GridEditController.Mode.Select)
            return;

        mIsHighlighted = true;

        if(!isSelected)
            RefreshHighlight();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(_isNonPlaceable || mode != GridEditController.Mode.Select)
            return;

        mIsHighlighted = false;

        if(!isSelected)
            RefreshHighlight();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(_isNonPlaceable || mode != GridEditController.Mode.Select)
            return;

        if(editController)
            editController.selected = this;
    }
        
    private void Init() {
        if(entity)
            entity.cellChangedCallback += RefreshBounds;

        if(editController)
            editController.changedCallback += RefreshMode;

        //refresh based on current edit state
        RefreshBounds();
        RefreshMode();

        mIsHighlighted = false;
    }

    private void Deinit() {
        if(entity)
            entity.cellChangedCallback -= RefreshBounds;

        if(mEditCtrl) {
            mEditCtrl.changedCallback -= RefreshMode;
            mEditCtrl = null;
        }
    }

    private void RefreshBounds() {
        var bounds = entity.bounds;

        //update collision
        var coll = collision;
        if(coll) {
            coll.center = new Vector3(0f, bounds.extents.y, 0f);
            coll.size = bounds.size;
        }
    }

    private void RefreshMode() {
        var ctrl = editController;
        if(!ctrl)
            return;

        mIsHighlighted = false;

        bool isCollEnabled = false;

        bool isFloorVisible = false;
        var fadeScale = 0f;

        switch(ctrl.mode) {
            case GridEditController.Mode.Select:
                isCollEnabled = true;

                fadeScale = 1f;

                if(isSelected)
                    mIsHighlighted = true;
                break;

            case GridEditController.Mode.Placement:
                fadeScale = GameData.instance.selectFadeScale;
                isFloorVisible = true;
                break;

            case GridEditController.Mode.Expand:
            case GridEditController.Mode.Move:
                //controls handled by GridGhostController, hide if we are the one selected
                if(!isSelected) {
                    fadeScale = GameData.instance.selectFadeScale;
                    isFloorVisible = true;
                }
                break;

            default: //off
                if(_isNonPlaceable)
                    fadeScale = 1f;
                break;
        }

        if(collision)
            collision.enabled = isCollEnabled;

        if(fadeScale > 0f) {
            display.isVisible = true;
            display.alpha = fadeScale;
            RefreshHighlight();
        }
        else
            display.isVisible = false;

        displayFloor.isVisible = isFloorVisible;
    }

    private void RefreshHighlight() {
        if(mIsHighlighted)
            display.pulseScale = GameData.instance.selectHighlightScale;
        else
            display.pulseScale = 0f;
    }
}
