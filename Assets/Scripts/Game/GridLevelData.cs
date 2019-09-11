using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Level Data")]
public class GridLevelData : ScriptableObject {
    [System.Serializable]
    public struct Goal {
        public GridEntityData data;
        public int volume; //volume based on 1 unit
        public int unitHeightRequire; //height based on 1 unit, set to 0 to have no height restriction
        public UnitMeasureType measureType;
    }

    public int resourceCount;
    public MixedNumber sideMeasure; //measurement per side
    public UnitMeasureType measureType = UnitMeasureType.Feet;

    public GridEntityData[] items;

    public Goal[] goals;

    public MixedNumber unitVolume { get { return sideMeasure * sideMeasure * sideMeasure; } }
}
