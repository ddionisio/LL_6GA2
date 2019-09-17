using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEditControllerModeShowHideVerify : GridEditControllerModeShowHide {

    protected override bool IsVisibleVerify() {
        //check if all goals are placed
        var goals = GridEditController.instance.levelData.goals;
        var container = GridEditController.instance.entityContainer;

        var entMatchCount = 0;

        for(int i = 0; i < goals.Length; i++) {
            var goal = goals[i];
            if(container.IsEntityContain(goal.data))
                entMatchCount++;
        }

        return entMatchCount == goals.Length;
    }
}
