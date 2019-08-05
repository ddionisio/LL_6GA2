using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrient : MonoBehaviour {
    public Transform target;

    //NOTE: starting from back dir of look-at

    public Transform defaultLookAt;
    public float distance;

    public M8.RangeFloat pitchRange; //angle limit for pitch rotation
    public float yDistance; //move as pitch change

    public float moveDelay; //delay when moving

    public Transform lookAt { get; private set; }

    public float pitchAngle {
        get { return mCurPitchAngle; }
        set {
            var a = pitchRange.Clamp(value);
            if(mCurPitchAngle != a) {
                mCurPitchAngle = a;
                RefreshDest();
            }
        }
    }

    public float yawAngle {
        get { return mCurYawAngle; }
        set {
            var a = value % 360f;
            if(mCurYawAngle != a) {
                mCurYawAngle = a;
                RefreshDest();
            }
        }
    }

    private Vector3 mDestForward;
    private Vector3 mDestPosition;

    private float mCurPitchAngle;
    private float mCurYawAngle;

    private Vector3 mVel;
    private Vector3 mVelForward;

    public void ApplyTelemetry(float yawAngle, float pitchAngle) {
        mCurYawAngle = yawAngle % 360f;
        mCurPitchAngle = pitchRange.Clamp(pitchAngle);
        RefreshDest();
    }

    public void SetLookAt(Transform t, bool immediate) {
        if(lookAt != t) {
            lookAt = t;

            if(lookAt) {
                mVel = Vector3.zero;
                mVelForward = Vector3.zero;

                mCurPitchAngle = pitchRange.min;
                mCurYawAngle = 0f;

                RefreshDest();

                if(immediate)
                    ApplyDestToCurrent();
            }
        }
    }

    public void RefreshDest() {
        var lookAtBack = GetBack();

        var pos = lookAt.position;
        pos.y += yDistance * pitchRange.GetT(mCurPitchAngle);

        mDestPosition = pos + lookAtBack * distance;
        mDestForward = -lookAtBack;
    }

    void OnEnable() {
        if(!target)
            target = transform;

        if(!lookAt && defaultLookAt)
            SetLookAt(defaultLookAt, true);
    }

    void Update() {
        if(!lookAt || !target)
            return;

        //check if we need to move
        var curForward = target.forward;
        if(curForward != mDestForward)
            target.forward = Vector3.SmoothDamp(curForward, mDestForward, ref mVelForward, moveDelay);

        var curPos = target.position;
        if(curPos != mDestPosition)
            target.position = Vector3.SmoothDamp(curPos, mDestPosition, ref mVel, moveDelay);

    }

    private Vector3 GetBack() {
        var lookAtBack = -lookAt.forward;
        lookAtBack = Quaternion.Euler(mCurPitchAngle, -mCurYawAngle, 0f) * lookAtBack;
        return lookAtBack;
    }
        
    private void ApplyDestToCurrent() {
        target.position = mDestPosition;
        target.forward = mDestForward;
    }
}
