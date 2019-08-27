using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LevelProgress : FsmStateAction {
        public override void OnEnter() {
            GameData.instance.ProceedToNextLevel();
            Finish();
        }
    }
}