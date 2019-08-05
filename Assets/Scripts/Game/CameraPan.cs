using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour {
    public Transform panTarget;
    public float panMoveDelay = 0.1f;

    public Transform boundTarget;
    public float boundWidth;
    public float boundLength;

    void Awake() {
        if(!panTarget)
            panTarget = transform;
    }

    void OnDrawGizmos() {
        if(boundTarget && boundWidth > 0f && boundLength > 0f) {
            Gizmos.color = Color.green;

            var pos = boundTarget.position;

            var hW = boundWidth * 0.5f;
            var hL = boundLength * 0.5f;

            Gizmos.DrawLine(new Vector3(pos.x - hW, pos.y, pos.z - hL), new Vector3(pos.x + hW, pos.y, pos.z - hL));
            Gizmos.DrawLine(new Vector3(pos.x + hW, pos.y, pos.z - hL), new Vector3(pos.x + hW, pos.y, pos.z + hL));
            Gizmos.DrawLine(new Vector3(pos.x + hW, pos.y, pos.z + hL), new Vector3(pos.x - hW, pos.y, pos.z + hL));
            Gizmos.DrawLine(new Vector3(pos.x - hW, pos.y, pos.z + hL), new Vector3(pos.x - hW, pos.y, pos.z - hL));
        }
    }
}
