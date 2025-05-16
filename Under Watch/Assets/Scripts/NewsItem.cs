using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NewsItem : MonoBehaviour
{
    //headline refs
    public TMP_Text titleText;
    public TMP_Text subTitleText;
    public GameObject notifPing;

    public NewsTitleManager newsMan;

    public NewsObject newsObj;

    public bool beenSeen;

    private void Start()
    {
        newsMan = FindObjectOfType<NewsTitleManager>();
    }
    public void ConfigureNewsItem(NewsObject news)
    {
        newsObj = news;
        titleText.text = news.title;
        subTitleText.text = news.subtitle;

        Debug.Log($"Prefs name: '{GetNewsPrefsName()}'");
        Debug.Log($"Day: {newsObj.day}, Title: '{newsObj.title}'");


        if (PlayerPrefs.GetInt(GetNewsPrefsName()) == 1)
        {
            newsObj.hasBeenSeen = true;
            Debug.Log("setting to 1");
        }
        else
        {
            Debug.Log("setting to 0");

            newsObj.hasBeenSeen = false;
            notifPing.SetActive(true);
            PlayerPrefs.SetInt("hasSeenNews", 0);
            PlayerPrefs.Save();

            NewsTitleManager.activateNewsPopup?.Invoke();
        }
    }
    public void ClickToDetails()
    {
        newsMan.clickedToDetails?.Invoke(newsObj);
        SetNewsSeen();
        newsObj.hasBeenSeen = true;
        notifPing.SetActive(false);

        NewsTitleManager.openDetails?.Invoke();
    }

    void SetNewsSeen()
    {
        PlayerPrefs.SetInt("hasSeenNews", 1);
        PlayerPrefs.SetInt(GetNewsPrefsName(), 1);
        PlayerPrefs.Save();

        Debug.Log(PlayerPrefs.GetInt(GetNewsPrefsName())); 
    }
    public string GetNewsPrefsName()
    {
        //TMPpro fields hide invisible characters, so clean up the data before saving it:
        string returnStr = $"{newsObj.day}_{newsObj.title.Trim().Replace("\u00A0", " ")}";
        returnStr = returnStr.Trim().Replace(" ", "");

        return returnStr;
    }
}
