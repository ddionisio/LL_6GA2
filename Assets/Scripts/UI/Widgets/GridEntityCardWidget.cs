using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridEntityCardWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Display")]
    public Image icon;
    public Text titleText;
    public GridEntityCardWidget cardDrag; //use for dragging

    public GridEntityData data { get; private set; }

    private static readonly GridCell spawnSize = new GridCell { b=1, row=1, col=1 };

    private bool mIsDragging;

    public void Setup(GridEntityData dat) {
        data = dat;

        if(icon) {
            icon.sprite = data.icon;
            //icon.SetNativeSize();
        }

        if(titleText) titleText.text = M8.Localize.Get(data.nameTextRef);

        if(cardDrag) cardDrag.gameObject.SetActive(false);
    }

    void OnApplicationFocus(bool focus) {
        if(!focus)
            DragEnd();
    }

    void OnDisable() {
        DragEnd();
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mIsDragging = true;

        if(cardDrag) {
            cardDrag.Setup(data);
            cardDrag.gameObject.SetActive(true);
        }

        var ghost = GridEditController.instance.ghostController;
        ghost.data = data;
        ghost.cellSize = spawnSize;

        GridEditController.instance.editMode = GridEditController.EditMode.Placement;

        DragUpdate(eventData);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        DragUpdate(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        DragEnd();

        //determine placement
        var editCtrl = GridEditController.instance;        
        var container = editCtrl.entityContainer;
        var levelGO = container.gameObject;
        var ctrl = container.controller;

        if(eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject == levelGO) {
            var worldPos = eventData.pointerCurrentRaycast.worldPosition;

            var cellInd = ctrl.GetCell(worldPos, false);

            var spawnEnt = data.Spawn(cellInd, spawnSize);
            if(spawnEnt) {
                //select and set mode to expand
                editCtrl.selected = spawnEnt;
                editCtrl.editMode = GridEditController.EditMode.Expand;
            }
        }
    }

    private void DragUpdate(PointerEventData eventData) {
        //update drag position
        if(cardDrag)
            cardDrag.transform.position = eventData.position;
                
        var ghost = GridEditController.instance.ghostController;
        var container = GridEditController.instance.entityContainer;
        var levelGO = container.gameObject;
        var ctrl = container.controller;

        if(eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject == levelGO) {

            var worldPos = eventData.pointerCurrentRaycast.worldPosition;

            var cellInd = ctrl.GetCell(worldPos, false);

            if(ctrl.IsContained(cellInd)) {
                ghost.mode = GridGhostController.Mode.None;

                ghost.cellIndex = cellInd;

                var isValid = cellInd.isValid && container.GetEntity(cellInd) == null;

                ghost.display.SetPulseColorValid(isValid);
            }
            else
                ghost.mode = GridGhostController.Mode.Hidden;
        }
        else
            ghost.mode = GridGhostController.Mode.Hidden;
    }

    private void DragEnd() {
        if(mIsDragging) {            
            if(cardDrag) cardDrag.gameObject.SetActive(false);

            var ghost = GridEditController.instance.ghostController;
            ghost.mode = GridGhostController.Mode.Hidden;

            //stop placement mode
            var editCtrl = GridEditController.instance;
            if(editCtrl.editMode == GridEditController.EditMode.Placement)
                editCtrl.editMode = GridEditController.EditMode.Select;

            mIsDragging = false;
        }
    }
}
