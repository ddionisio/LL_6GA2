using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalControlWidget : MonoBehaviour {
    [System.Serializable]
    public class ReqData {
        public GameObject rootGO;
        public GameObject correctGO;
        public GameObject incorrectGO;
        public Text text;

        public void Setup(string str, bool isMet) {
            text.text = str;

            correctGO.SetActive(isMet);
            incorrectGO.SetActive(!isMet);
        }
    }

    [Header("Display")]
    public Image icon;
    public Text titleText;
    public Text volumeText;
    public ReqData[] reqs;
    public Text efficiencyText;
    public Text errorText;
    public Button prevButton;
    public Button nextButton;

    [Header("Text Refs")]
    [M8.Localize]
    public string totalVolumeTextRef;
    [M8.Localize]
    public string reqVolumeTextRef;
    [M8.Localize]
    public string reqHeightTextRef;
    [M8.Localize]
    public string efficiencyTextRef;
    [M8.Localize]
    public string errorNoMatchTextRef;
    [M8.Localize]
    public string errorVolumeTextRef;
    [M8.Localize]
    public string errorHeightTextRef;

    private System.Text.StringBuilder mSB = new System.Text.StringBuilder();

    void OnDestroy() {
        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= OnEditChanged;
    }

    void Awake() {
        GridEditController.instance.editChangedCallback += OnEditChanged;

        prevButton.onClick.AddListener(OnClickPrev);
        nextButton.onClick.AddListener(OnClickNext);
    }

    void OnClickPrev() {
        var editCtrl = GridEditController.instance;
        if(editCtrl.currentEvaluateIndex > 0) {
            editCtrl.currentEvaluateIndex--;
            RefreshDisplay();
        }
    }

    void OnClickNext() {
        var editCtrl = GridEditController.instance;
        if(editCtrl.currentEvaluateIndex < editCtrl.goalEvaluations.Length - 1) {
            editCtrl.currentEvaluateIndex++;
            RefreshDisplay();
        }
    }

    void OnEditChanged() {
        var editCtrl = GridEditController.instance;
        if(editCtrl.editMode == GridEditController.EditMode.Evaluate) {
            editCtrl.currentEvaluateIndex = 0;
            RefreshDisplay();
        }
    }

    private void RefreshDisplay() {
        var editCtrl = GridEditController.instance;

        var curEvalInd = editCtrl.currentEvaluateIndex;

        var curEval = editCtrl.goalEvaluations[curEvalInd];
        var curGoal = editCtrl.levelData.goals[curEvalInd];

        //header
        icon.sprite = curGoal.data.icon;
        titleText.text = M8.Localize.Get(curGoal.data.nameTextRef);

        //total volume
        mSB.Clear();
        mSB.Append(M8.Localize.Get(totalVolumeTextRef));
        mSB.Append(' ');

        if(curEval.isValid)
            mSB.Append(UnitMeasure.GetVolumeText(editCtrl.levelData.measureType, curEval.volume));
        else
            mSB.Append("-- " + UnitMeasure.GetVolumeText(editCtrl.levelData.measureType));

        volumeText.text = mSB.ToString();

        var isVolumeMet = curEval.GoalIsVolumeMet(curGoal);
        var isHeightMet = curEval.GoalIsHeightMet(curGoal);

        //volume req
        mSB.Clear();
        mSB.Append(M8.Localize.Get(reqVolumeTextRef));
        mSB.Append(' ');
        mSB.Append(UnitMeasure.GetVolumeText(curGoal.measureType, curGoal.volume));
        reqs[0].Setup(mSB.ToString(), isVolumeMet);

        //height req
        if(curGoal.heightRequire > 0f) {
            mSB.Clear();
            mSB.Append(M8.Localize.Get(reqHeightTextRef));
            mSB.Append(' ');
            mSB.Append(UnitMeasure.GetMeasureText(curGoal.measureType, curGoal.heightRequire));
            reqs[1].Setup(mSB.ToString(), isHeightMet);
            reqs[1].rootGO.SetActive(true);
        }
        else
            reqs[1].rootGO.SetActive(false);

        if(isVolumeMet && isHeightMet) { //valid, show efficiency
            var s = curEval.GoalEfficiencyScale(curGoal);
            var percent = Mathf.RoundToInt(s * 100f);

            mSB.Clear();
            mSB.Append(M8.Localize.Get(efficiencyTextRef));
            mSB.Append(' ');
            mSB.Append(percent);
            mSB.Append('%');
            efficiencyText.text = mSB.ToString();

            efficiencyText.gameObject.SetActive(true);
            errorText.gameObject.SetActive(false);
        }
        else { //error, show message
            if(!curEval.isValid)
                errorText.text = M8.Localize.Get(errorNoMatchTextRef);
            else if(!isVolumeMet)
                errorText.text = M8.Localize.Get(errorVolumeTextRef);
            else if(!isHeightMet)
                errorText.text = string.Format(M8.Localize.Get(errorHeightTextRef), UnitMeasure.GetMeasureText(curGoal.measureType, curGoal.heightRequire));
            else
                errorText.text = "";

            efficiencyText.gameObject.SetActive(false);
            errorText.gameObject.SetActive(true);
        }
    }
}
