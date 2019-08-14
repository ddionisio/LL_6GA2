using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Entity Data Group")]
public class GridEntityDataGroup : ScriptableObject {
    [System.Serializable]
    public struct Item {
        public GridEntityData data;
        public int count;
    }

    public Item[] items;
}
