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

    private System.Text.StringBuilder mSB = new System.Text.StringBuilder();

    public void Setup(GridLevelData.Goal goal) {
        icon.sprite = goal.data.icon;
        titleText.text = M8.Localize.Get(goal.data.nameTextRef);

        mSB.Clear();

        mSB.Append("· ");
        mSB.Append(M8.Localize.Get(volumeReqTextRef));
        mSB.Append(' ');
        mSB.Append(UnitMeasure.GetVolumeText(goal.measureType, goal.volume));

        if(goal.heightRequire > 0f) {
            mSB.Append('\n');
            mSB.Append("· ");
            mSB.Append(M8.Localize.Get(heightReqTextRef));
            mSB.Append(' ');
            mSB.Append(UnitMeasure.GetMeasureText(goal.measureType, goal.heightRequire));
        }

        descText.text = mSB.ToString();
    }
}
