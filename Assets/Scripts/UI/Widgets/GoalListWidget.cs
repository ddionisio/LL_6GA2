using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalListWidget : MonoBehaviour {
    [Header("Template")]
    public GoalItemWidget template;

    [Header("Display")]
    public Transform container;

    void Awake() {
        template.gameObject.SetActive(false);

        var goals = GridEditController.instance.levelData.goals;
        for(int i = 0; i < goals.Length; i++) {
            var goal = goals[i];

            var itm = Instantiate(template, container);
            itm.Setup(goal);
            itm.gameObject.SetActive(true);
        }
    }
}
