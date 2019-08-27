using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelProgressCurrent : FsmStateAction {
        public override void OnEnter() {
            GameData.instance.ProceedFromProgress();
            Finish();
        }
    }
}