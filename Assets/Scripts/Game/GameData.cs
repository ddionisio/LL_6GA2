using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [System.Serializable]
    public struct RankData {
        public string text;
        public float scale;
        public Color color;
    }

    [Header("Data")]
    public int efficiencyScore = 1000;
    public int bonusScore = 1000;
    public int bonusPenalty = 100;
    public RankData[] ranks; //highest to lowest

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

    public RankData GetRank(int score) {
        float perfectScore = efficiencyScore + bonusScore;
        float fScore = score;

        var scale = Mathf.Clamp01(fScore / perfectScore);

        for(int i = 0; i < ranks.Length; i++) {
            var rank = ranks[i];

            if(scale >= rank.scale)
                return rank;
        }

        return ranks[ranks.Length - 1];
    }

    public void SaveCurLevelScore(int score) {
        var ind = GetLevelIndex();
        if(ind == -1)
            return;

        M8.SceneState.instance.global.SetValue("levelScore" + ind, score, false);
    }

    public int GetLevelScore(int level) {
        return M8.SceneState.instance.global.GetValue("levelScore" + level);
    }

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

    public int GetLevelIndex() {
        var curScene = M8.SceneManager.instance.curScene;

        int curInd = -1;
        for(int i = 0; i < levels.Length; i++) {
            if(levels[i] == curScene) {
                curInd = i;
                break;
            }
        }

        return curInd;
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
