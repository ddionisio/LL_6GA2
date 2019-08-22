using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour {
    [SerializeField]
    Transform _lookTarget;
    [SerializeField]
    float _lookDistance; //distance to the bound area
    [SerializeField]
    float _lookAnglePitch; //pitch angle from bound area

    public float lookMoveDelay = 0.1f;

    public Transform boundTarget;
    public Vector3 bounds; //pivot at bottom

    [Header("Signal Listen")]
    public M8.SignalVector3 signalListenPanTo;
    public M8.SignalVector3 signalListenLookAtDelta;
    public M8.SignalFloat signalListenLookYawDelta;

    public Transform lookTarget {
        get { return _lookTarget; }
        set {
            _lookTarget = value;
        }
    }

    public float lookDistance {
        get { return _lookDistance; }
        set {
            if(_lookDistance != value) {
                _lookDistance = value;
                RefreshPanPosition();
            }
        }
    }

    public float lookAnglePitch {
        get { return _lookAnglePitch; }
        set {
            var a = value % 360f;
            if(_lookAnglePitch != a) {
                _lookAnglePitch = a;
                RefreshPanPosition();
            }
        }
    }
        
    public float lookAngleYaw {
        get { return mLookAngleYaw; }
        set {
            var a = value % 360f;
            if(mLookAngleYaw != a) {
                mLookAngleYaw = a;
                RefreshPanPosition();
            }
        }
    }

    public Vector3 lookPosition { get; private set; }
    public Vector3 lookDir { get; private set; }

    public Vector3 lookAtLocalPosition {
        get { return mLookAtLocPos; }
        set {
            //clamp
            var hBoundX = bounds.x * 0.5f;
            var hBoundZ = bounds.z * 0.5f;

            var p = new Vector3(Mathf.Clamp(value.x, -hBoundX, hBoundX), Mathf.Clamp(value.y, 0f, bounds.y), Mathf.Clamp(value.z, -hBoundZ, hBoundZ));
            if(mLookAtLocPos != p) {
                mLookAtLocPos = p;
                RefreshPanPosition();
            }
        }
    }

    private Vector3 mLookAtLocPos = Vector3.zero;
    private float mLookAngleYaw = 0f;

    private Vector3 mLookCurVel;
    private Vector3 mLookCurDirVel;

    void OnDisable() {
        if(signalListenPanTo)
            signalListenPanTo.callback -= OnSignalPanTo;

        if(signalListenLookAtDelta)
            signalListenLookAtDelta.callback -= OnSignalLookAtMoveDelta;

        if(signalListenLookYawDelta)
            signalListenLookYawDelta.callback -= OnSignalLookYawDelta;
    }

    void OnEnable() {
        //reset
        mLookAtLocPos = Vector3.zero;
        mLookAngleYaw = 0f;

        mLookCurVel = Vector3.zero;
        mLookCurDirVel = Vector3.zero;

        RefreshPanPosition();

        if(signalListenPanTo)
            signalListenPanTo.callback += OnSignalPanTo;

        if(signalListenLookAtDelta)
            signalListenLookAtDelta.callback += OnSignalLookAtMoveDelta;

        if(signalListenLookYawDelta)
            signalListenLookYawDelta.callback += OnSignalLookYawDelta;
    }

    void Awake() {
        if(!boundTarget)
            boundTarget = transform;
    }

    void Update() {
        if(!lookTarget)
            return;

        var curPos = lookTarget.position;
        if(curPos != lookPosition)
            lookTarget.position = Vector3.SmoothDamp(curPos, lookPosition, ref mLookCurVel, lookMoveDelay);

        var curDir = lookTarget.forward;
        if(curDir != lookDir)
            lookTarget.forward = Vector3.SmoothDamp(curDir, lookDir, ref mLookCurDirVel, lookMoveDelay);
    }

    void OnSignalPanTo(Vector3 pos) {
        var lpos = boundTarget.InverseTransformPoint(pos);
        lookAtLocalPosition = lpos;
    }

    void OnSignalLookAtMoveDelta(Vector3 delta) {
        var r = _lookTarget.right;
        var xDelta = new Vector2(r.x * delta.x, r.z * delta.x);

        var f = _lookTarget.forward; f.y = 0f; f.Normalize();
        var zDelta = new Vector2(f.x * delta.z, f.z * delta.z);

        var d = new Vector3(xDelta.x + zDelta.x, delta.y, xDelta.y + zDelta.y);

        lookAtLocalPosition += d;
    }

    void OnSignalLookYawDelta(float a) {
        lookAngleYaw += a;
    }

    private void RefreshPanPosition() {
        if(!boundTarget)
            return;

        var boundPos = boundTarget.TransformPoint(lookAtLocalPosition);

        var boundBackDir = -boundTarget.forward;
        boundBackDir = Quaternion.Euler(lookAnglePitch, lookAngleYaw, 0f) * boundBackDir;

        lookDir = -boundBackDir;
        lookPosition = boundPos + boundBackDir * lookDistance;
    }

    void OnDrawGizmosSelected() {
        if(boundTarget) {
            //draw bounds
            if(bounds.x > 0f && bounds.y > 0f && bounds.z > 0f) {
                Gizmos.color = Color.green;
                M8.Gizmo.DrawWireCube(transform, new Vector3(0f, bounds.y * 0.5f, 0f), bounds * 0.5f);
            }

            //draw positions
            var boundPos = boundTarget.TransformPoint(lookAtLocalPosition);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(boundPos, 0.1f);

            if(!Application.isPlaying)
                RefreshPanPosition();

            Gizmos.DrawLine(boundPos, lookPosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lookPosition, 0.1f);
        }
    }
}
