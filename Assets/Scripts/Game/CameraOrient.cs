using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrient : MonoBehaviour {
    public Transform target;

    //NOTE: starting from back dir of look-at
    public Transform defaultLookAt;
    public float distance;
    public Vector3 startRotation; //rotate from target to camera
    public M8.RangeFloat pitchRange; //angle limit for pitch rotation
    public float yDistance; //move as pitch change

    public float yawScale; //scale from yaw-delta provided

    public float gotoDelay; //delay when going to new target
    public float moveDelay; //delay when moving

    public Transform lookAt { get; private set; }

    public bool isBusy { get { return mRout != null; } }

    private Vector3 mDestForward;
    private Vector3 mDestPosition;

    private Coroutine mRout;

    public void SetTarget(Transform t, bool immediate) {
        lookAt = t;

        //move camera towards target
    }

    void OnEnable() {
        
    }
}
