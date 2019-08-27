using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.ModalController, M8.IModalPush {
    [Header("Display")]
    public Text efficiencyText;
    public Text scoreText;

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

        //apply score
        LoLManager.instance.curScore += score;

        //setup display
        efficiencyText.text = string.Format("{0}%", efficiencyPercent);
        scoreText.text = score.ToString();
    }
}
