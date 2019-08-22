using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Level Data")]
public class GridLevelData : ScriptableObject {
    [System.Serializable]
    public struct Goal {
        public GridEntityData data;
        public float volume;
        public float heightRequire; //set to 0 to have no height restriction
        public UnitMeasureType measureType;
    }

    public int resourceCount;
    public float sideMeasure = 1f; //measurement per side
    public UnitMeasureType measureType = UnitMeasureType.Feet;

    public GridEntityData[] items;

    public Goal[] goals;
}
