using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrient : MonoBehaviour {

    public Transform defaultTarget;
    public float distance;
    public Vector3 startRotation; //rotate from target to camera
    public M8.RangeFloat pitchRange; //angle limit for pitch rotation

    private Transform mTarget;

    public void SetTarget(Transform t) {
        mTarget = t;

        //move camera towards target
    }

    void OnEnable() {
        
    }
}
