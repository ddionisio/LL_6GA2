using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show build button once we get to the last evaluation
/// </summary>
public class GoalBuildShowWidget : MonoBehaviour {
    public GameObject displayGO;

    void OnDestroy() {
        if(GridEditController.isInstantiated) {
            GridEditController.instance.editChangedCallback -= OnRefresh;
            GridEditController.instance.evalCurrentChangedCallback -= OnRefresh;
        }
    }

    void Awake() {
        displayGO.SetActive(false);

        GridEditController.instance.editChangedCallback += OnRefresh;
        GridEditController.instance.evalCurrentChangedCallback += OnRefresh;
    }

    void OnRefresh() {
        var editCtrl = GridEditController.instance;
        if(editCtrl.editMode == GridEditController.EditMode.Evaluate) {
            //check if we have all goals met
            if(editCtrl.isAllGoalsMet) {
                //check if we are at the last index
                if(editCtrl.currentEvaluateIndex >= editCtrl.goalEvaluations.Length - 1)
                    displayGO.SetActive(true);
            }
            else
                displayGO.SetActive(false);
        }
    }
}
