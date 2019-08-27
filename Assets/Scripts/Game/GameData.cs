using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [Header("Data")]
    public int efficiencyScore = 1000;

    [Header("Levels")]
    public M8.SceneAssetPath[] levels;
    public M8.SceneAssetPath end;

    [Header("Edit Config")]
    public float anchorOffset = 0.15f; //offset from top of entity
    public float panningScaleX = -0.01f;
    public float panningScaleZ = -0.01f;

    [Header("Mesh Config")]
    public float textureTile = 1f;

    [Header("Display Config")]
    public float selectHighlightPulseScale = 0.5f;
    public float selectPulseScale = 0.7f;
    public float selectFadeScale = 0.2f;

    public float floorAlpha = 0.5f;

    /// <summary>
    /// Proceed based on progress from LoL
    /// </summary>
    public void ProceedFromProgress() {
        var curProgress = LoLManager.instance.curProgress;

        if(curProgress < levels.Length)
            levels[curProgress].Load();
        else
            end.Load();
    }

    /// <summary>
    /// Go to the next level, will also apply progress to LoL
    /// </summary>
    public void ProceedToNextLevel() {
        var curScene = M8.SceneManager.instance.curScene;

        int curInd = -1;
        for(int i = 0; i < levels.Length; i++) {
            if(levels[i] == curScene) {
                curInd = i;
                break;
            }
        }

        if(curInd == -1) {
            levels[0].Load();
        }
        else {
            curInd++;
            LoLManager.instance.ApplyProgress(curInd);

            if(curInd < levels.Length)
                levels[curInd].Load();
            else
                end.Load();
        }
    }
}
