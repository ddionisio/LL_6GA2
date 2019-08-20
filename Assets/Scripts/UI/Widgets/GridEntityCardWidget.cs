using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridEntityCardWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Display")]
    public Image icon;
    public Text titleText;
    public Text countText;
    public Color countValidColor;
    public Color countInvalidColor;
    public Selectable selectable; //used for disabling when count is 0

    public GridEntityData data { get; private set; }

    [Header("Signal Listen")]
    public SignalGridEntity signalListenEntitySizeChanged;
    public SignalGridEntityData signalListenMapChanged;

    private static readonly GridCell spawnSize = new GridCell { b=1, row=1, col=1 };

    private GridEntityCardWidget mDragWidget;
    private bool mIsDragging;

    public void SetCount(int count) {
        if(!countText)
            return;

        countText.text = count.ToString();

        if(count > 0) {
            selectable.interactable = true;
            countText.color = countValidColor;
        }
        else {
            selectable.interactable = false;
            countText.color = countInvalidColor;
        }
    }

    public void RefreshCount() {
        var count = GridEditController.instance.GetAvailableCount(data);
        SetCount(count);
    }
        
    public void Setup(GridEntityData dat, GridEntityCardWidget dragWidget) {
        data = dat;
        mDragWidget = dragWidget;

        if(icon) {
            icon.sprite = data.icon;
            icon.SetNativeSize();
        }

        if(titleText) titleText.text = M8.Localize.Get(data.nameTextRef);
    }

    void OnApplicationFocus(bool focus) {
        if(!focus)
            DragEnd();
    }

    void OnDisable() {
        DragEnd();

        if(signalListenEntitySizeChanged)
            signalListenEntitySizeChanged.callback -= OnEntitySizeChanged;

        if(signalListenMapChanged)
            signalListenMapChanged.callback -= OnMapChanged;
    }

    void OnEnable() {
        RefreshCount();

        if(signalListenEntitySizeChanged)
            signalListenEntitySizeChanged.callback += OnEntitySizeChanged;

        if(signalListenMapChanged)
            signalListenMapChanged.callback += OnMapChanged;
    }

    void OnEntitySizeChanged(GridEntity ent) {
        if(ent.data == data)
            RefreshCount();
    }

    void OnMapChanged(GridEntityData entData) {
        if(entData == data)
            RefreshCount();
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(selectable && !selectable.interactable)
            return;

        mIsDragging = true;

        if(mDragWidget) {
            mDragWidget.Setup(data, null);
            mDragWidget.gameObject.SetActive(true);
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
        if(mDragWidget)
            mDragWidget.transform.position = eventData.position;
                
        var ghost = GridEditController.instance.ghostController;
        var container = GridEditController.instance.entityContainer;
        var levelGO = container.gameObject;
        var ctrl = container.controller;

        if(eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject == levelGO) {
            ghost.mode = GridGhostController.Mode.None;

            var worldPos = eventData.pointerCurrentRaycast.worldPosition;

            var cellInd = ctrl.GetCell(worldPos, false);

            ghost.cellIndex = cellInd;

            var isValid = cellInd.isValid && container.GetEntity(cellInd) == null;

            ghost.display.SetPulseColorValid(isValid);
        }
        else
            ghost.mode = GridGhostController.Mode.Hidden;
    }

    private void DragEnd() {
        if(mIsDragging) {            
            if(mDragWidget) mDragWidget.gameObject.SetActive(false);

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
