using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuildTilePopulator : MonoBehaviour {
    [Header("Config")]
    public float probabilityScale = 0.3f; //probability to spawn, if random lands to [0, probabilityScale], then spawn
    public M8.RangeFloat delayRange;
    public Vector2 randomRange;
    public M8.RangeFloat rotateRange = new M8.RangeFloat { min=0f, max=360f };

    [Header("Templates")]
    public GameObject[] templates;

    [Header("Signal Listen")]
    public M8.Signal signalListenExecute;

    private bool mIsActive;

    void OnDisable() {
        if(mIsActive) {
            signalListenExecute.callback -= OnExecute;
            mIsActive = false;
        }
    }

    void OnEnable() {
        signalListenExecute.callback += OnExecute;
        mIsActive = true;
    }

    void OnExecute() {
        signalListenExecute.callback -= OnExecute;
        mIsActive = false;

        if(Random.value > probabilityScale)
            return;

        var template = templates[Random.Range(0, templates.Length)];
        if(!template)
            return;

        StartCoroutine(DoExecute(template));
    }

    IEnumerator DoExecute(GameObject template) {
        yield return new WaitForSeconds(delayRange.random);

        var goInst = Instantiate(template, transform);
        var t = goInst.transform;

        t.localPosition = new Vector3(Random.Range(-randomRange.x, randomRange.x), 0f, Random.Range(-randomRange.y, randomRange.y));
        t.localEulerAngles = new Vector3(0f, rotateRange.random, 0f);
    }

    void OnDrawGizmos() {
        var pt0 = transform.TransformPoint(-randomRange.x, 0f, -randomRange.y);
        var pt1 = transform.TransformPoint(-randomRange.x, 0f, randomRange.y);
        var pt2 = transform.TransformPoint(randomRange.x, 0f, randomRange.y);
        var pt3 = transform.TransformPoint(randomRange.x, 0f, -randomRange.y);

        Gizmos.color = Color.white;

        Gizmos.DrawLine(pt0, pt1);
        Gizmos.DrawLine(pt1, pt2);
        Gizmos.DrawLine(pt2, pt3);
        Gizmos.DrawLine(pt3, pt0);
    }
}
