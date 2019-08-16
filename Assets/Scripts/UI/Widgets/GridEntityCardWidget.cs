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
    public Selectable selectable; //used for disabling when count is 0

    public GridEntityData data { get; private set; }

    private GridEntityCardWidget mDragWidget;
    private bool mIsDragging;

    public void UpdateCount(int count) {
        if(count > 0) {

        }
        else {
            EndDrag(); //if we are currently dragging for some reason
        }
    }

    public void Setup(GridEntityData aData, GridEntityCardWidget dragWidget) {
        data = aData;
        mDragWidget = dragWidget;


    }

    void OnApplicationFocus(bool focus) {
        if(!focus)
            EndDrag();
    }

    void OnDisable() {
        EndDrag();
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(!selectable.interactable)
            return;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;
    }

    private void EndDrag() {
        if(mIsDragging) {
            //stop placement mode
            var editCtrl = GameData.instance.gridEditController;
            if(editCtrl.mode == GridEditController.Mode.Placement)
                editCtrl.mode = GridEditController.Mode.Select;
        }

        mIsDragging = false;
        if(mDragWidget) mDragWidget.gameObject.SetActive(false);
    }
}
