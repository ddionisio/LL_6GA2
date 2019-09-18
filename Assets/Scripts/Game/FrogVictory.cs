using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogVictory : MonoBehaviour {
    [Header("Data")]
    public M8.RangeFloat spawnDelay;
    public M8.RangeFloat idleDelay;
    public M8.RangeFloat jumpRange;
    public float jumpDelay;
    public DG.Tweening.Ease jumpUpEase = DG.Tweening.Ease.OutSine;
    public DG.Tweening.Ease jumpDownEase = DG.Tweening.Ease.InSine;

    [Header("Display")]
    public GameObject displayGO;
    public Transform frogRoot;
    public SpriteRenderer frogRender;
    public SpriteRenderer frogHatRender;

    [Header("Sprite")]
    public Sprite[] frogHatSprites; //set one as empty for no hat
    public Sprite frogIdleSprite;
    public Sprite frogJumpSprite;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;

    [Header("Signal Listen")]
    public M8.Signal signalListenActivate;

    void OnDisable() {
        signalListenActivate.callback -= OnSignalActivate;
    }

    void OnEnable() {
        displayGO.SetActive(false);

        signalListenActivate.callback += OnSignalActivate;
    }

    void OnSignalActivate() {
        var hatSprite = frogHatSprites[Random.Range(0, frogHatSprites.Length)];
        if(hatSprite) {
            frogHatRender.gameObject.SetActive(true);
            frogHatRender.sprite = hatSprite;
        }
        else
            frogHatRender.gameObject.SetActive(false);

        StartCoroutine(DoActive());
    }

    IEnumerator DoActive() {
        var jumpUpEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(jumpUpEase);
        var jumpDownEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(jumpDownEase);
        var jumpHDelay = jumpDelay * 0.5f;

        yield return new WaitForSeconds(spawnDelay.random);

        frogRender.sprite = frogIdleSprite;
        Flip();

        displayGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);

        while(true) {
            //jump
            frogRender.sprite = frogJumpSprite;

            var lpos = frogRoot.localPosition;

            var height = jumpRange.random;

            //up
            var curTime = 0f;
            while(curTime < jumpHDelay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = jumpUpEaseFunc(curTime, jumpHDelay, 0f, 0f);

                lpos.y = Mathf.Lerp(0f, height, t);

                frogRoot.localPosition = lpos;
            }

            //down
            curTime = 0f;
            while(curTime < jumpHDelay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = jumpDownEaseFunc(curTime, jumpHDelay, 0f, 0f);

                lpos.y = Mathf.Lerp(height, 0f, t);

                frogRoot.localPosition = lpos;
            }

            lpos.y = 0f;
            frogRoot.localPosition = lpos;

            //wait
            frogRender.sprite = frogIdleSprite;
            Flip();

            yield return new WaitForSeconds(idleDelay.random);
        }
    }

    private void Flip() {
        if(Random.value <= 0.5f) {
            var s = frogRoot.localScale;
            s.x *= -1f;
            frogRoot.localScale = s;
        }
    }
}
