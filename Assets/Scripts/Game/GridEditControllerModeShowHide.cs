using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show or Hide depending on edit controller mode
/// </summary>
public class GridEditControllerModeShowHide : MonoBehaviour {
    [Header("Data")]
    public GridEditController.EditMode[] visibleModes;

    [Header("Display")]
    public GameObject displayGO;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    private Coroutine mRout;
    private bool mIsVisible;

    void OnDisable() {
        mRout = null;

        if(GridEditController.isInstantiated)
            GridEditController.instance.editChangedCallback -= OnEditControllerChanged;
    }

    void OnEnable() {
        var editCtrl = GridEditController.instance;
        editCtrl.editChangedCallback += OnEditControllerChanged;

        RefreshDisplay(true);
    }

    void OnEditControllerChanged() {
        RefreshDisplay(false);
    }

    IEnumerator DoShow() {
        displayGO.SetActive(true);

        if(animator) {
            while(animator.isPlaying)
                yield return null;

            if(!string.IsNullOrEmpty(takeEnter))
                yield return animator.PlayWait(takeEnter);
        }

        mRout = null;
    }

    IEnumerator DoHide() {
        if(animator) {
            while(animator.isPlaying)
                yield return null;

            if(!string.IsNullOrEmpty(takeExit))
                yield return animator.PlayWait(takeExit);
        }

        displayGO.SetActive(false);

        mRout = null;
    }

    private void RefreshDisplay(bool forceApply) {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        var editCtrl = GridEditController.instance;
        var curMode = editCtrl.editMode;

        bool isVisible = false;

        for(int i = 0; i < visibleModes.Length; i++) {
            if(curMode == visibleModes[i]) {
                isVisible = true;
                break;
            }
        }

        if(forceApply || mIsVisible != isVisible) {
            mIsVisible = isVisible;

            if(mIsVisible)
                mRout = StartCoroutine(DoShow());
            else {
                if(forceApply)
                    displayGO.SetActive(false);
                else
                    mRout = StartCoroutine(DoHide());
            }
        }
    }
}
