using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuildController : MonoBehaviour {

    [Header("Signal Invoke")]
    public M8.Signal signalInvokeBuildComplete;

    void OnDestroy() {
        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= OnEditModeChanged;
    }

    void Awake() {
        GridEditController.instance.editChangedCallback += OnEditModeChanged;
    }

    void OnEditModeChanged() {
        if(GridEditController.instance.editMode == GridEditController.EditMode.Build) {
            StartCoroutine(DoBuild());
        }
    }

    IEnumerator DoBuild() {
        yield return null;

        //do stuff

        if(signalInvokeBuildComplete)
            signalInvokeBuildComplete.Invoke();
    }
}
