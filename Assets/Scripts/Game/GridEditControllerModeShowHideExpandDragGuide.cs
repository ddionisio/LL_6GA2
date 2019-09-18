using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEditControllerModeShowHideExpandDragGuide : GridEditControllerModeShowHide {
    [Header("Drag Guide")]
    public GridCell cellSize;

    public DragToGuideWidget dragGuideWidget;

    public GameObject confirmExpandGO;

    private Camera mCamera;

    protected override void OnHide() {
        dragGuideWidget.Hide();

        confirmExpandGO.SetActive(false);
    }

    void Awake() {
        confirmExpandGO.SetActive(false);
    }

    void Update() {
        if(isVisible) {
            var gridCtrl = GridEditController.instance.entityContainer.controller;
            var ghostCtrl = GridEditController.instance.ghostController;

            if(!mCamera)
                mCamera = Camera.main;

            bool isDragVisible = false;
            Vector3 startPosWorld = Vector3.zero, endPosWorld = Vector3.zero;

            //check if col size is met, show expand from right side
            if(ghostCtrl.cellSize.col < cellSize.col) {
                startPosWorld = ghostCtrl.expandCollRight.transform.position;

                GridCell toCell = new GridCell();

                toCell.row = ghostCtrl.cellIndex.row + (ghostCtrl.cellSize.row / 2);
                toCell.col = ghostCtrl.cellIndex.col + cellSize.col - 1;

                var toCellBounds = gridCtrl.GetBoundsFromCell(toCell);
                endPosWorld = gridCtrl.transform.TransformPoint(new Vector3(toCellBounds.center.x, 0f, ghostCtrl.transform.localPosition.z));
                
                isDragVisible = true;
            }
            //check row, show expand from back side
            else if(ghostCtrl.cellSize.row < cellSize.row) {
                startPosWorld = ghostCtrl.expandCollBack.transform.position;

                GridCell toCell = new GridCell();

                toCell.row = ghostCtrl.cellIndex.row - (cellSize.row - ghostCtrl.cellSize.row);
                toCell.col = ghostCtrl.cellIndex.col + (ghostCtrl.cellSize.col / 2);

                var toCellBounds = gridCtrl.GetBoundsFromCell(toCell);
                endPosWorld = gridCtrl.transform.TransformPoint(new Vector3(ghostCtrl.transform.localPosition.x, 0f, toCellBounds.center.z));

                isDragVisible = true;
            }

            if(isDragVisible) {
                endPosWorld.y = startPosWorld.y;

                Vector2 dragStart = mCamera.WorldToScreenPoint(startPosWorld);
                Vector2 dragEnd = mCamera.WorldToScreenPoint(endPosWorld);

                if(!dragGuideWidget.isActive)
                    dragGuideWidget.Show(false, dragStart, dragEnd);
                else
                    dragGuideWidget.UpdatePositions(dragStart, dragEnd);

                confirmExpandGO.SetActive(false);
            }
            else {
                dragGuideWidget.Hide();

                confirmExpandGO.SetActive(true);
            }
        }
    }
}
