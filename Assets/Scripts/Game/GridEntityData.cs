using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Grid Entity Data")]
public class GridEntityData : ScriptableObject {
    [Header("Info")]
    public Sprite icon;
    [M8.Localize]
    public string nameTextRef;
    [M8.Localize]
    public string descTextRef;

    [Header("Placement Info")]
    public Material placementMaterial; //material used during placement mode
    public Color placementColor; //color used during placement
}
