using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.ModalController, M8.IModalPush {
    [Header("Display")]
    public Text efficiencyText;

    public M8.UI.Texts.TextCounter scoreText;
    public M8.UI.Texts.TextCounter bonusText;
    public M8.UI.Texts.TextCounter totalText;

    public Text rankText;

    [Header("SFX")]
    [M8.SoundPlaylist]
    public string sfxVictory;

    void M8.IModalPush.Push(M8.GenericParams parms) {
        //get average efficiency
        var efficiencyScale = 0f;

        var evals = GridEditController.instance.goalEvaluations;
        var goals = GridEditController.instance.levelData.goals;
        for(int i = 0; i < evals.Length; i++) {
            var eval = evals[i];
            var goal = goals[i];

            efficiencyScale += eval.GoalEfficiencyScale(goal);
        }

        efficiencyScale /= evals.Length;

        var score = Mathf.RoundToInt(GameData.instance.efficiencyScore * efficiencyScale);
        var efficiencyPercent = Mathf.RoundToInt(efficiencyScale * 100f);

        var bonus = Mathf.Clamp(GameData.instance.bonusScore - GameData.instance.bonusPenalty * GridEditController.instance.returnCount, 0, GameData.instance.bonusScore);

        var totalScore = score + bonus;

        GameData.instance.SaveCurLevelScore(totalScore);

        //apply score
        LoLManager.instance.curScore += totalScore;

        //setup display
        efficiencyText.text = string.Format("{0}%", efficiencyPercent);

        scoreText.count = score;
        bonusText.count = bonus;
        totalText.count = totalScore;

        var rankData = GameData.instance.GetRank(totalScore);

        rankText.text = rankData.text;
        rankText.color = rankData.color;

        if(!string.IsNullOrEmpty(sfxVictory))
            M8.SoundPlaylist.instance.Play(sfxVictory, false);
    }
}
