using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class NewsButton : MonoBehaviour
{
    public GameObject newsPopup;
    public GameObject iconNotif;

    public bool hasSeenNews = false;

    public Vector2 buttonPos;
    public Vector2 popupPos;
    void Start()
    {
        //otherwise, it is inactive
        if (PlayerPrefs.GetInt("hasSeenNews") == 0)
        {
            iconNotif.SetActive(true);
        }

        //buttonPos = new Vector2(this.transform.position.x, this.transform.position.y);

      buttonPos = new Vector3(300, 700, 0);
      popupPos = new Vector2(0, 0);

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

            PlayerPrefs.SetInt("hasSeenNews", 1);
            iconNotif.SetActive(false);
        }
    }
}
