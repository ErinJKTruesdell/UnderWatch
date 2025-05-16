using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;
using Unity.VisualScripting;
public class NewsObject
{
    public int day;
    public string title;
    public string subtitle;
    public string desc;
    public bool hasBeenSeen;
    public NewsObject(int _day, string _title, string _subtitle, string _desc, bool seen = false)
    {
        day = _day;
        title = _title;
        subtitle = _subtitle;
        desc = _desc;
        hasBeenSeen = seen;
    }
}
public class NewsTitleManager : MonoBehaviour
{
    //to add a news object, go to inspector and create a new NewsDataContainer
    public UnityEvent<NewsObject> clickedToDetails = new();
    public Transform contentParent;
    public GameObject newsPrefab;

    public GameObject detailsWindow;
    public GameObject detailsCloser;
    public TMP_Text detailsTitleText;
    public TMP_Text detailsDescText;

    public Transform containersParent;
    public NewsDataContainer[] newsDataContainers;

    public List<NewsObject> currentDayNewsObjects = new();
    public List<NewsObject> AllNewsObjects = new();

    public static UnityEvent activateNewsPopup = new();
    public static UnityEvent openDetails = new();
    private void Awake()
    {
        newsDataContainers = containersParent.GetComponentsInChildren<NewsDataContainer>();
    }
    private void OnEnable()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        clickedToDetails.AddListener(OpenDetailsWindow);
        SpawnNewsObjs();
    }
    private void OnDisable()
    {
        clickedToDetails.RemoveAllListeners();
    }
    private void SpawnNewsObjs()
    {
        //clear the UI elements
        //currentDayNewsObjects.Clear();

        AddAllNewsData();

        if (AllNewsObjects.Count > 0)
        {
            Debug.Log(AllNewsObjects.Count + "count");
            PlayerPrefs.SetInt("hasSeenNews", 1);
            PlayerPrefs.Save();

            foreach (NewsObject news in AllNewsObjects)
            {
                GameObject newsObj = Instantiate(newsPrefab) as GameObject;
                newsObj.transform.SetParent(contentParent);
                newsObj.transform.localScale = Vector3.one;

                NewsItem newsData = newsObj.GetComponent<NewsItem>();
                newsData.ConfigureNewsItem(news);
            }
            Debug.Log("pref: " + PlayerPrefs.GetInt("hasSeenNews"));
        }
    }

    private void AddAllNewsData()
    {
        int day = DayManager.currentDay;
        List<GameObject> containersToDestroy = new();

        foreach (NewsDataContainer newsData in newsDataContainers)
        {
            if (newsData == null || newsData.gameObject == null)
                continue;

            bool alreadyExists = AllNewsObjects.Exists(n => n.title == newsData.title && n.day == newsData.day);
            if (alreadyExists)
            {
                containersToDestroy.Add(newsData.gameObject);
            }
            else if (newsData.day <= day)
            {
                NewsObject newsObj = new(newsData.day, newsData.title, newsData.subtitle, newsData.desc);
                currentDayNewsObjects.Add(newsObj);
                AllNewsObjects.Add(newsObj);
            }
        }

        // Now destroy safely
        foreach (GameObject go in containersToDestroy)
        {
            Destroy(go);
        }
        // Refresh the array so you're not holding references to destroyed components
        newsDataContainers = containersParent.GetComponentsInChildren<NewsDataContainer>();
    }
    private void OpenDetailsWindow(NewsObject newsObj)
    {
        detailsCloser.SetActive(true);
        detailsWindow.SetActive(true);

        detailsTitleText.text = newsObj.title;
        detailsDescText.text = newsObj.desc;

        if (PlayerPrefs.GetInt("hasSeenNews") == 0)
        {
            RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.announcements);
        }
    }
    public void CloseDetailsWindow()
    {
        detailsCloser.SetActive(false);
        detailsWindow.SetActive(false);
    }
}

