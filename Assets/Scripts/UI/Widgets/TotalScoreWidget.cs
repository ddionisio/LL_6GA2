using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TotalScoreWidget : MonoBehaviour {
    public Text rankText;
    public M8.UI.Texts.TextCounter scoreText;

    void OnEnable() {
        var score = LoLManager.instance.curScore;

        scoreText.count = score;

        var avgScore = score / GameData.instance.levels.Length;

        var rankDat = GameData.instance.GetRank(avgScore);

        rankText.text = rankDat.text;
        rankText.color = rankDat.color;
    }
}
