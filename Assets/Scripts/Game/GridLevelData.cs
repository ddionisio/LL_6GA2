using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Level Data")]
public class GridLevelData : ScriptableObject {
    [System.Serializable]
    public struct Goal {
        public GridEntityData data;
    }

    public int resourceCount;

    public GridEntityData[] items;

    public Goal[] goals;
}
