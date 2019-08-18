using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class GridEditSetMode : FsmStateAction {
        public GridEditController.EditMode mode;

        public override void Reset() {
            mode = GridEditController.EditMode.None;
        }

        public override void OnEnter() {
            GridEditController.instance.editMode = mode;

            Finish();
        }
    }
}