using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SignalInvokePanningDrag : MonoBehaviour, IDragHandler {
    [Header("Signal Invoke")]
    public M8.SignalVector3 signalInvokeDelta;

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var delta = eventData.delta;

        if(signalInvokeDelta)
            signalInvokeDelta.Invoke(new Vector3(delta.x * GameData.instance.panningScaleX, 0f, delta.y * GameData.instance.panningScaleZ));
    }
}
