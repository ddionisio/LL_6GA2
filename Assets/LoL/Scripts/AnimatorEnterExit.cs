﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEnterExit : MonoBehaviour {
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    public bool isPlaying { get { return animator ? animator.isPlaying : false; } }

    public void PlayEnter() {
        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.Play(takeEnter);
    }

    public IEnumerator PlayEnterWait() {
        if(animator && !string.IsNullOrEmpty(takeEnter)) {
            animator.Play(takeEnter);
            while(animator.isPlaying)
                yield return null;
        }
    }

    public void PlayExit() {
        if(animator && !string.IsNullOrEmpty(takeExit))
            animator.Play(takeExit);
    }

    public IEnumerator PlayExitWait() {
        if(animator && !string.IsNullOrEmpty(takeExit)) {
            animator.Play(takeExit);
            while(animator.isPlaying)
                yield return null;
        }
    }
}