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

    [Header("SFX")]
    [M8.SoundPlaylist]
    public string sfxOpen;
    [M8.SoundPlaylist]
    public string sfxClose;

    public bool isVisible { get; private set; }

    private Coroutine mRout;
    
    protected virtual bool IsVisibleVerify() {
        return true;
    }

    protected virtual void OnShow() { }

    protected virtual void OnHide() { }

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
        yield return null;

        if(animator) {
            var takeLast = animator.currentPlayingTakeName;

            while(animator.isPlaying)
                yield return null;

            if(!string.IsNullOrEmpty(sfxOpen))
                M8.SoundPlaylist.instance.Play(sfxOpen, false);

            if(!string.IsNullOrEmpty(takeEnter) && takeLast != takeEnter)
                yield return animator.PlayWait(takeEnter);
        }

        mRout = null;
    }

    IEnumerator DoHide() {
        yield return null;

        if(animator) {
            var takeLast = animator.currentPlayingTakeName;

            while(animator.isPlaying)
                yield return null;

            if(!string.IsNullOrEmpty(sfxClose))
                M8.SoundPlaylist.instance.Play(sfxClose, false);

            if(!string.IsNullOrEmpty(takeExit) && takeLast != takeExit)
                yield return animator.PlayWait(takeExit);
        }

        displayGO.SetActive(false);

        mRout = null;
    }

    protected void RefreshDisplay(bool forceApply) {
        var editCtrl = GridEditController.instance;
        var curMode = editCtrl.editMode;

        bool _isVisible = false;

        for(int i = 0; i < visibleModes.Length; i++) {
            if(curMode == visibleModes[i]) {
                _isVisible = true;
                break;
            }
        }

        if(_isVisible)
            _isVisible = IsVisibleVerify();

        if(forceApply || isVisible != _isVisible) {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            isVisible = _isVisible;

            if(isVisible) {
                OnShow();

                displayGO.SetActive(true);

                mRout = StartCoroutine(DoShow());
            }
            else {
                OnHide();

                if(forceApply)
                    displayGO.SetActive(false);
                else
                    mRout = StartCoroutine(DoHide());
            }
        }
    }
}
