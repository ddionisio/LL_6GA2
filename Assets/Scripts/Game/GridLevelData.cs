using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Level Data")]
public class GridLevelData : ScriptableObject {
    [System.Serializable]
    public struct Item {
        public GridEntityData data;
        public int count;
    }

    [System.Serializable]
    public struct Goal {
        public GridEntityData data;
    }

    public Item[] items;

    public Goal[] goals;

    public int GetItemCount(GridEntityData data) {
        for(int i = 0; i < items.Length; i++) {
            var itm = items[i];
            if(itm.data == data)
                return itm.count;
        }

        return 0;
    }
}
