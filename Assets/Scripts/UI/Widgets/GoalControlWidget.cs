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

        public void Setup(string str, bool isMet, Color color) {
            text.text = str;
            text.color = color;

            correctGO.SetActive(isMet);
            incorrectGO.SetActive(!isMet);
        }
    }

    [Header("Display")]
    public Image icon;
    public Text titleText;
    public Text volumeText;
    public ReqData[] reqs;
    public Color reqCorrectColor;
    public Color reqIncorrectColor;
    public Text efficiencyText;
    public Text errorText;
    public Button prevButton;
    public Button nextButton;

    public GameObject buildReadyGO;

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

    [Header("Signal Invoke")]
    public M8.SignalVector3 signalInvokeCameraPanTo;
    public M8.Signal signalInvokeCameraYawReset;

    private System.Text.StringBuilder mSB = new System.Text.StringBuilder();

    private int mCurrentEvaluateIndex;
    private int mToEvaluateIndex;

    void OnDestroy() {
        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= OnEditChanged;
    }

    void Awake() {
        GridEditController.instance.editChangedCallback += OnEditChanged;

        prevButton.onClick.AddListener(OnClickPrev);
        nextButton.onClick.AddListener(OnClickNext);

        buildReadyGO.SetActive(false);
    }

    void LateUpdate() {
        if(mCurrentEvaluateIndex != mToEvaluateIndex) {
            EvaluateApplyFocus(mToEvaluateIndex);
            RefreshDisplay();
        }
    }

    void OnClickPrev() {
        var editCtrl = GridEditController.instance;
        if(mCurrentEvaluateIndex > 0) {
            mToEvaluateIndex = mCurrentEvaluateIndex - 1;
        }
    }

    void OnClickNext() {
        var editCtrl = GridEditController.instance;
        if(mCurrentEvaluateIndex < editCtrl.goalEvaluations.Length - 1) {
            mToEvaluateIndex = mCurrentEvaluateIndex + 1;
        }
    }

    void OnEditChanged() {
        var editCtrl = GridEditController.instance;
        if(editCtrl.editMode == GridEditController.EditMode.Evaluate) {
            if(signalInvokeCameraYawReset)
                signalInvokeCameraYawReset.Invoke();

            mCurrentEvaluateIndex = -1;
            mToEvaluateIndex = 0;
        }
    }

    private void RefreshDisplay() {
        var editCtrl = GridEditController.instance;

        var curEval = editCtrl.goalEvaluations[mCurrentEvaluateIndex];
        var curGoal = editCtrl.levelData.goals[mCurrentEvaluateIndex];

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
        reqs[0].Setup(mSB.ToString(), isVolumeMet, isVolumeMet ? reqCorrectColor : reqIncorrectColor);

        //height req
        if(curGoal.heightRequire > 0f) {
            mSB.Clear();
            mSB.Append(M8.Localize.Get(reqHeightTextRef));
            mSB.Append(' ');
            mSB.Append(UnitMeasure.GetMeasureText(curGoal.measureType, curGoal.heightRequire));
            reqs[1].Setup(mSB.ToString(), isHeightMet, isHeightMet ? reqCorrectColor : reqIncorrectColor);
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

        if(editCtrl.goalEvaluations.Length == 1) {
            prevButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
        }
        else if(mCurrentEvaluateIndex <= 0) {
            prevButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
        }
        else if(mCurrentEvaluateIndex >= editCtrl.goalEvaluations.Length - 1) {
            prevButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);
        }

        //check if we have all goals met
        if(editCtrl.isAllGoalsMet) {
            //check if we are at the last index
            if(mCurrentEvaluateIndex >= editCtrl.goalEvaluations.Length - 1)
                buildReadyGO.SetActive(true);
        }
        else
            buildReadyGO.SetActive(false);
    }

    private void EvaluateApplyFocus(int toIndex) {
        var editCtrl = GridEditController.instance;

        GridEditController.EvaluateData eval;

        if(mCurrentEvaluateIndex != -1) {
            //clear out focus from previous
            eval = editCtrl.goalEvaluations[mCurrentEvaluateIndex];
            eval.SetAlpha(GameData.instance.selectFadeScale);
            eval.SetPulseAlpha(0f);
        }

        mCurrentEvaluateIndex = toIndex;

        //apply focus
        eval = editCtrl.goalEvaluations[mCurrentEvaluateIndex];
        if(eval.isValid) {
            eval.SetAlpha(1f);
            eval.SetPulseAlpha(GameData.instance.selectPulseScale);

            if(signalInvokeCameraPanTo) {
                var b = eval.bounds;
                var focusPt = new Vector3(b.center.x, b.min.y, b.center.z);
                signalInvokeCameraPanTo.Invoke(focusPt);
            }
        }
    }
}
