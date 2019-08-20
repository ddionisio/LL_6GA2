using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [Header("Edit Config")]
    public float anchorOffset = 0.15f; //offset from top of entity
    public float panningScaleX = -0.01f;
    public float panningScaleZ = -0.01f;

    [Header("Mesh Config")]
    public float textureTile = 1f;

    [Header("Display Config")]
    public float selectHighlightScale = 1f;
    public float selectFadeScale = 0.2f;

    public float floorAlpha = 0.5f;
}
