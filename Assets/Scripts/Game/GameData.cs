using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [Header("Edit Config")]
    [M8.TagSelector]
    public string tagEditController;
    [M8.TagSelector]
    public string tagEditGhostController;

    public float anchorOffset = 0.15f; //offset from top of entity

    [Header("Display Config")]
    public float selectHighlightScale = 1f;
    public float selectFadeScale = 0.2f;

    public float floorAlpha = 0.5f;

    public GridEditController gridEditController {
        get {
            if(!mGridEditController) {
                var go = GameObject.FindGameObjectWithTag(tagEditController);
                if(go)
                    mGridEditController = go.GetComponent<GridEditController>();
            }

            return mGridEditController;
        }
    }

    public GridGhostController gridEditGhostController {
        get {
            if(!mGridEditGhostController) {
                if(!mGridEditGhostController) {
                    var go = GameObject.FindGameObjectWithTag(tagEditGhostController);
                    if(go)
                        mGridEditGhostController = go.GetComponent<GridGhostController>();
                }
            }

            return mGridEditGhostController;
        }
    }

    private GridEditController mGridEditController;
    private GridGhostController mGridEditGhostController;
}
