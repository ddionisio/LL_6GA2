using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraOrientInputDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public CameraOrient target;

    [Header("Data")]
    public float yawScale;
    public float pitchScale;

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {

    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var delta = eventData.delta;
        if(delta != Vector2.zero) {
            var curPitch = target.pitchAngle;
            var curYaw = target.yawAngle;

            var deltaPitch = delta.y * pitchScale;
            var deltaYaw = delta.x * yawScale;

            target.ApplyTelemetry(curYaw + deltaYaw, curPitch + deltaPitch);
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {

    }
}
