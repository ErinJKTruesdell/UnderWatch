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
    public NewsTitleManager newsMan;

    public NewsObject newsObj;

    private void Start()
    {
        newsMan = FindObjectOfType<NewsTitleManager>();
    }
    public void ConfigureNewsItem(NewsObject news)
    {
        newsObj = news;
        titleText.text = news.title;
        subTitleText.text = news.subtitle;
    }
    public void ClickToDetails()
    {
        newsMan.clickedToDetails?.Invoke(newsObj);
    }
}
