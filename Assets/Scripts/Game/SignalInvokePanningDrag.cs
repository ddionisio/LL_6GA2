using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SignalInvokePanningDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Signal Invoke")]
    public M8.SignalVector3 signalInvokeDelta;

    private bool mIsDragging;

    void OnApplicationFocus(bool focus) {
        if(!focus)
            mIsDragging = false;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        mIsDragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        //only drag on specific mode
        switch(GridEditController.instance.editMode) {
            case GridEditController.EditMode.None:
            case GridEditController.EditMode.Evaluate:
                return;
        }

        var delta = eventData.delta;

        if(signalInvokeDelta)
            signalInvokeDelta.Invoke(new Vector3(delta.x * GameData.instance.panningScaleX, 0f, delta.y * GameData.instance.panningScaleZ));
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        mIsDragging = false;
    }
}
