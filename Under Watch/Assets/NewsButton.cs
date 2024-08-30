using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NewsButton : MonoBehaviour
{
    public GameObject newsPopup;
    public GameObject iconNotif;

    public bool hasSeenNews = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("hasSeenNews") == 0)
        {
            iconNotif.SetActive(true);
        }
        //otherwise, it is inactive
    }
    public void OpenNews()
    {
        newsPopup.SetActive(true);

        PlayerPrefs.SetInt("hasSeenNews", 1);
        iconNotif.SetActive(false);
    }

    public void CloseNews()
    {
        newsPopup.SetActive(false);
    }
}
