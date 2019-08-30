using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitMeasureInfoWidget : MonoBehaviour {
    [Header("Data")]
    [M8.Localize]
    public string sidesTextRef;
    [M8.Localize]
    public string volumeTextRef;

    [Header("Display")]
    public Text headerText;
    public Text infoText;

    void Awake() {
        var sb = new System.Text.StringBuilder();

        //apply header display
        sb.AppendLine(M8.Localize.Get(sidesTextRef));
        sb.Append(M8.Localize.Get(volumeTextRef));

        headerText.text = sb.ToString();

        //apply info display
        var editCtrl = GridEditController.instance;
        var measureType = editCtrl.levelData.measureType;
        var side = editCtrl.levelData.sideMeasure;
        var volume = side * side * side;

        sb.Clear();
        sb.AppendLine(UnitMeasure.GetMeasureText(measureType, side));
        sb.Append(UnitMeasure.GetVolumeText(measureType, volume));

        infoText.text = sb.ToString();
    }
}
