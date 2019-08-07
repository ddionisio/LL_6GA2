using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanInputWidget : MonoBehaviour {
    [Header("Data")]
    public float angleDelta = 45f;
    public float yDelta = 0.5f;

    [Header("Signal Invoke")]
    public M8.SignalVector3 signalInvokeMoveDelta;
    public M8.SignalFloat signalInvokeRotateYawDelta;

    public void MoveUp() {
        signalInvokeMoveDelta.Invoke(new Vector3(0f, yDelta, 0f));
    }

    public void MoveDown() {
        signalInvokeMoveDelta.Invoke(new Vector3(0f, -yDelta, 0f));
    }

    public void RotateLeft() {
        signalInvokeRotateYawDelta.Invoke(-angleDelta);
    }

    public void RotateRight() {
        signalInvokeRotateYawDelta.Invoke(angleDelta);
    }
}
