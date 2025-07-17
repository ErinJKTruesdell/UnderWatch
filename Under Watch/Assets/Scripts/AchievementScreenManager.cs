using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;

public class AchievementScreenManager : MonoBehaviour
{
    public Transform leftScroll;
    public Transform rightScroll;
    public Image leftBar;
    public Image rightBar;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;

    public string savedScreenName = "saved_screen";
    public float offscreenPost = 1000;
    public Color offBarColor = new(0, 0, 0, .2f);
    public Coroutine animationCoroutine;
    private Tween activeTargetTween;
    private Tween activeOffTween;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private bool isSwiping = false;

    [SerializeField] private float swipeThreshold = 50f; // Adjust as needed

    void OnEnable()
    {
        Debug.Log("color: " + rightText.color);
        if (PlayerPrefs.GetInt(savedScreenName) == 1)
        {
            //we're on right screen
            SetScrollsPosition(rightScroll, leftScroll, true);
            SetBarColor(rightBar, rightText, leftBar, leftText);
        }
        else
        {
            //we're on left screen
            SetScrollsPosition(leftScroll, rightScroll, false);
            SetBarColor(leftBar, leftText, rightBar, rightText);
        }
    }

    public void GoToLeftScreen()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        KillActiveTweens(); // interrupt any in-progress animation

        animationCoroutine = StartCoroutine(AnimateScrollPosition(leftScroll, rightScroll, false));
        SetBarColor(leftBar, leftText, rightBar, rightText);
    }

    public void GoToRightScreen()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        KillActiveTweens(); // interrupt any in-progress animation

        animationCoroutine = StartCoroutine(AnimateScrollPosition(rightScroll, leftScroll, true));
        SetBarColor(rightBar, rightText, leftBar, leftText);
    }

    IEnumerator AnimateScrollPosition(Transform targetScroll, Transform offScroll, bool isRightTarget)
    {
        // ensure target screen is visible before animating
        targetScroll.gameObject.SetActive(true);

        // interrupt current position and animate to new
        activeTargetTween = targetScroll.DOLocalMoveX(0, 0.3f).SetEase(Ease.InOutQuad);
        if (isRightTarget)
        {
            activeOffTween = offScroll.DOLocalMoveX(-offscreenPost, 0.3f).SetEase(Ease.InOutQuad)
                .OnComplete(() => offScroll.gameObject.SetActive(false));
        }
        else
        {
            activeOffTween = offScroll.DOLocalMoveX(offscreenPost, 0.3f).SetEase(Ease.InOutQuad)
                .OnComplete(() => offScroll.gameObject.SetActive(false));
        }

        yield return activeTargetTween.WaitForCompletion();
        yield return activeOffTween.WaitForCompletion();

        PlayerPrefs.SetInt(savedScreenName, Convert.ToInt32(isRightTarget));
    }
    void KillActiveTweens()
    {
        activeTargetTween?.Kill();
        activeOffTween?.Kill();
        activeTargetTween = null;
        activeOffTween = null;
    }

    void SetScrollsPosition(Transform targetScroll, Transform offScroll, bool isRightTarget)
    {
        targetScroll.transform.localPosition = new Vector3(0, 0, 0);
        if (isRightTarget == true)
            offScroll.transform.localPosition = new Vector3(-offscreenPost, 0, 0);
        else
            offScroll.transform.localPosition = new Vector3(offscreenPost, 0, 0);

        targetScroll.gameObject.SetActive(true);
        offScroll.gameObject.SetActive(false);
    }

    void Update()
    {
        // Touch input (mobile)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isSwiping = true;
                    touchStartPos = touch.position;
                    break;

                case TouchPhase.Ended:
                    if (!isSwiping) break;

                    touchEndPos = touch.position;
                    Vector2 delta = touchEndPos - touchStartPos;

                    if (Mathf.Abs(delta.x) > swipeThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x > 0)
                            GoToLeftScreen();
                        else
                            GoToRightScreen();
                    }

                    isSwiping = false;
                    break;
            }
        }

        // Mouse input (for desktop testing)
        if (Input.GetMouseButtonDown(0))
        {
            isSwiping = true;
            touchStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!isSwiping) return;

            touchEndPos = Input.mousePosition;
            Vector2 delta = touchEndPos - touchStartPos;

            if (Mathf.Abs(delta.x) > swipeThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x > 0)
                    GoToLeftScreen();
                else
                    GoToRightScreen();
            }

            isSwiping = false;
        }
    }

    void SetBarColor(Image targetBar, TextMeshProUGUI targetText, Image offBar, TextMeshProUGUI offText)
    {
        targetBar.color = GameManager.blueCol;
        offBar.color = offBarColor;

        targetText.color = GameManager.blueCol;
        offText.color = GameManager.pinkCol;
    }
}
