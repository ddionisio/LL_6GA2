using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEditControllerModeShowHidePlacementDragGuide : GridEditControllerModeShowHide {
    [Header("Drag Guide")]
    public GridEntityData data;
    public GridCell toCell;

    public GridEntityDeckWidget deckWidget;
    public DragToGuideWidget dragGuideWidget;

    private Camera mCamera;

    private GridEntityCardWidget mCardWidget;

    protected override bool IsVisibleVerify() {
        //check if given data is already placed
        var container = GridEditController.instance.entityContainer;
        return !container.IsEntityContain(data);
    }

    protected override void OnShow() {
        //show drag guide to designated cell
        mCardWidget = null;
        for(int i = 0; i < deckWidget.cards.Count; i++) {
            var card = deckWidget.cards[i];
            if(card.data == data) {
                mCardWidget = card;
                break;
            }
        }
    }

    protected override void OnHide() {
        //hide drag guide
        dragGuideWidget.Hide();
    }

    void Update() {
        if(isVisible && mCardWidget) {
            var startPos = mCardWidget.transform.position;
            var endPos = GetDragEndPos();

            if(dragGuideWidget.isActive)
                dragGuideWidget.UpdatePositions(startPos, endPos);
            else
                dragGuideWidget.Show(false, startPos, endPos);
        }
    }

    private Vector2 GetDragEndPos() {
        if(!mCamera)
            mCamera = Camera.main;

        var gridCtrl = GridEditController.instance.entityContainer.controller;

        var cellBounds = gridCtrl.GetBoundsFromCell(toCell);
        var endPosWorld = gridCtrl.transform.TransformPoint(new Vector3(cellBounds.center.x, cellBounds.min.y, cellBounds.center.z));

        return mCamera.WorldToScreenPoint(endPosWorld);
    }
}
