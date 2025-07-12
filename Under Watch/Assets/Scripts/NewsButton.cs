using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class NewsButton : MonoBehaviour
{
    public GameObject newsPopup;
    public GameObject iconNotif;

    public bool hasSeenNews = false;
    public static bool firstLoad = true;

    public Vector2 buttonPos;
    public Vector2 popupPos;

    void OnEnable()
    {
        StartCoroutine(delayedInit());

        //buttonPos = new Vector2(this.transform.position.x, this.transform.position.y);

          buttonPos = new Vector3(300, 700, 0);
          popupPos = new Vector2(0, 0);
        NewsTitleManager.activateNewsPopup.AddListener(HandleNotifPing);
        NewsTitleManager.openDetails.AddListener(HandleNotifPing);
    }
    private void OnDisable()
    {
        NewsTitleManager.activateNewsPopup.RemoveListener(HandleNotifPing);
        NewsTitleManager.openDetails.RemoveListener(HandleNotifPing);
    }
    void HandleNotifPing()
    {
        if (PlayerPrefs.GetInt("hasSeenNews") == 0 || ((DayManager.DoesDayContainObjective(DayManager.ObjTypes.privacyPolicy) || DayManager.DoesDayContainObjective(DayManager.ObjTypes.announcements)) && firstLoad))
            iconNotif.SetActive(true);
        else
            iconNotif.SetActive(false);
    }
    IEnumerator delayedInit()
    {
        yield return new WaitForSeconds(.3f);
        //otherwise, it is inactive
        if (PlayerPrefs.GetInt("hasSeenNews") == 0 || ((DayManager.DoesDayContainObjective(DayManager.ObjTypes.privacyPolicy) || DayManager.DoesDayContainObjective(DayManager.ObjTypes.announcements)) && firstLoad))
        {
            firstLoad = false;
            iconNotif.SetActive(true);
            Debug.Log("hasnt seen it!" + PlayerPrefs.GetInt("hasSeenNews"));
        }
    }
    public void OpenNews()
    {
        if (newsPopup.activeSelf)
        {
            newsPopup.transform.DOScale(new Vector3(0, 0, 0), .2f);
            newsPopup.transform.DOLocalMove(buttonPos, .3f).SetEase(Ease.OutCubic).
                OnComplete(() => newsPopup.SetActive(false));
        }
        else
        {
            newsPopup.SetActive(true);
            newsPopup.transform.localScale = new Vector3(0, 0, 0);
            newsPopup.transform.position = buttonPos;

            newsPopup.transform.DOScale(new Vector3(1, .05f, 1), .6f);
            newsPopup.transform.DOLocalMove(popupPos, .4f).SetEase(Ease.OutCubic).
                OnComplete(() => newsPopup.transform.DOScale(new Vector3(1, 1, 1), .2f));
        }
        HandleNotifPing();
    }
}
