using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridEditControllerDeselectClick : MonoBehaviour, IPointerClickHandler, IBeginDragHandler {

    private bool mIsDrag;

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            mIsDrag = false;
        }
    }
    
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(mIsDrag) {
            mIsDrag = false;
            return;
        }

        if(GridEditController.instance.editMode == GridEditController.EditMode.Select)
            GridEditController.instance.selected = null;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mIsDrag = true;
    }

    /*void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        mIsDrag = false;
    }*/
}
