using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalItemWidget : MonoBehaviour {
    [Header("Data")]
    [M8.Localize]
    public string volumeReqTextRef;
    [M8.Localize]
    public string heightReqTextRef;

    [Header("Display")]
    public Image icon;
    public Text titleText;
    public Text descText;
    public Text valueText;

    private System.Text.StringBuilder mSB = new System.Text.StringBuilder();

    public void Setup(GridLevelData.Goal goal) {
        icon.sprite = goal.data.icon;
        titleText.text = M8.Localize.Get(goal.data.nameTextRef);

        mSB.Clear();

        mSB.Append("· ");
        mSB.Append(M8.Localize.Get(volumeReqTextRef));

        var editCtrl = GridEditController.instance;

        var goalVolume = goal.volume * editCtrl.levelData.unitVolume;
        //goalVolume.SimplifyImproper();
        //goalVolume.Simplify();
        goalVolume.FractionToWhole();

        var goalHeight = goal.unitHeightRequire * editCtrl.levelData.sideMeasure;
        goalHeight.SimplifyImproper();

        if(goalHeight.fValue > 0f) {
            mSB.Append('\n');
            mSB.Append("· ");
            mSB.Append(M8.Localize.Get(heightReqTextRef));
        }

        descText.text = mSB.ToString();

        mSB.Clear();

        mSB.Append(UnitMeasure.GetVolumeText(goal.measureType, goalVolume));

        if(goalHeight.fValue > 0f) {
            mSB.Append('\n');
            mSB.Append(UnitMeasure.GetMeasureText(goal.measureType, goalHeight));
        }

        valueText.text = mSB.ToString();
    }
}
