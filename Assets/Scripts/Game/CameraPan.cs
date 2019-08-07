using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour {
    [SerializeField]
    Transform _panTarget;
    [SerializeField]
    float _panDistance; //distance to the pan area
    [SerializeField]
    float _panAnglePitch; //pitch angle from pan area

    public float panMoveDelay = 0.1f;

    public Transform boundTarget;
    public Vector3 bounds; //pivot at bottom

    [Header("Signal Listen")]
    public M8.SignalVector3 signalListenBoundMoveDelta;

    public Transform panTarget {
        get { return _panTarget; }
        set {
            _panTarget = value;
        }
    }

    public float panDistance {
        get { return _panDistance; }
        set {
            if(_panDistance != value) {
                _panDistance = value;
                RefreshPanPosition();
            }
        }
    }

    public float panAnglePitch {
        get { return _panAnglePitch; }
        set {
            var a = value % 360f;
            if(_panAnglePitch != a) {
                _panAnglePitch = a;
                RefreshPanPosition();
            }
        }
    }
        
    public float panAngleYaw {
        get { return mPanAngleYaw; }
        set {
            var a = value % 360f;
            if(mPanAngleYaw != a) {
                mPanAngleYaw = a;
                RefreshPanPosition();
            }
        }
    }

    public Vector3 panPosition { get; private set; }
    public Vector3 panDir { get; private set; }

    public Vector3 boundLocalPosition {
        get { return mBoundLocPos; }
        set {
            //clamp
            var hBoundX = bounds.x * 0.5f;
            var hBoundZ = bounds.z * 0.5f;

            var p = new Vector3(Mathf.Clamp(value.x, -hBoundX, hBoundX), Mathf.Clamp(value.y, 0f, bounds.y), Mathf.Clamp(value.z, -hBoundZ, hBoundZ));
            if(mBoundLocPos != p) {
                mBoundLocPos = p;
                RefreshPanPosition();
            }
        }
    }

    private Vector3 mBoundLocPos = Vector3.zero;
    private float mPanAngleYaw = 0f;

    private Vector3 mPanCurVel;
    private Vector3 mPanCurDirVel;

    void OnEnable() {
        //reset
        mBoundLocPos = Vector3.zero;
        mPanAngleYaw = 0f;

        mPanCurVel = Vector3.zero;
        mPanCurDirVel = Vector3.zero;

        RefreshPanPosition();
    }

    void Awake() {
        if(!boundTarget)
            boundTarget = transform;
    }

    void Update() {
        if(!panTarget)
            return;

        var curPos = panTarget.position;
        if(curPos != panPosition)
            panTarget.position = Vector3.SmoothDamp(curPos, panPosition, ref mPanCurVel, panMoveDelay);

        var curDir = panTarget.forward;
        if(curDir != panDir)
            panTarget.forward = Vector3.SmoothDamp(curDir, panDir, ref mPanCurDirVel, panMoveDelay);
    }

    private void RefreshPanPosition() {
        if(!boundTarget)
            return;

        var boundPos = boundTarget.position + boundLocalPosition;

        var boundBackDir = -boundTarget.forward;
        boundBackDir = Quaternion.Euler(panAnglePitch, panAngleYaw, 0f) * boundBackDir;

        panDir = -boundBackDir;
        panPosition = boundPos + boundBackDir * panDistance;
    }

    void OnDrawGizmosSelected() {
        if(boundTarget) {
            var pos = boundTarget.position;

            //draw bounds
            if(bounds.x > 0f && bounds.y > 0f && bounds.z > 0f) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(new Vector3(pos.x, pos.y + bounds.y * 0.5f, pos.z), bounds);
            }

            //draw positions
            var boundPos = pos + boundLocalPosition;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(boundPos, 0.1f);

            if(!Application.isPlaying)
                RefreshPanPosition();

            Gizmos.DrawLine(boundPos, panPosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(panPosition, 0.1f);
        }
    }
}
