using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraPanInputDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public float panScaleX = 0.1f;
    public float panScaleZ = 0.1f;

    [Header("Signal Invoke")]
    public M8.SignalVector3 signalInvokeDelta;

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {

    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var delta = eventData.delta;

        if(signalInvokeDelta)
            signalInvokeDelta.Invoke(new Vector3(delta.x * panScaleX, 0f, delta.y * panScaleZ));
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {

    }
}
