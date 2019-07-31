using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class SignalBooleanWait : FsmStateAction {
        [ObjectType(typeof(SignalBoolean))]
        [RequiredField]
        public FsmObject signal;

        public FsmEvent trueEvent;
        public FsmEvent falseEvent;

        private SignalBoolean mSignal;

        public override void OnEnter() {
            mSignal = (SignalBoolean)signal.Value;
            mSignal.callback += OnSignal;
        }

        public override void OnExit() {
            if(mSignal)
                mSignal.callback -= OnSignal;
        }

        void OnSignal(bool b) {
            Fsm.Event(b ? trueEvent : falseEvent);
            Finish();
        }
    }
}