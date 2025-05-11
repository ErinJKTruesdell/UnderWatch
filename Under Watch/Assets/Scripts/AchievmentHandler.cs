using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AchievementsManager : MonoBehaviour
{
    //to create a new achievement:
    //add the object in the achievements list, create an int[] for the tiers, ensure there is an int counting, then fill out the UpdateAchievement in OnEnable

    public Color32 blueCol = new(99, 202, 225, 255);
    public Color32 pinkCol = new(237, 30, 121, 255);
    public Color32 redCol = new(180, 17, 75, 255);

    public GameObject notif;
    public RectTransform notifRect;
    public TextMeshProUGUI notifDesc;

    public AchieveMonitor ach;
    public GameManager gm;

    //super supporter, social butterfly, ispymaster
    public List<GameObject> achievements = new();

    //achievement tiers
    public int[] superSupporterTiers = { 5, 15, 30, 50 };
    public int[] socialButterflyTiers = { 5, 15, 30, 50 };
    public int[] iSpyMasterTiers = { 5, 15, 30, 50 };

    private Vector3 notifStartPos;

    public void Start()
    {
        notifStartPos = notif.transform.localPosition;
        notifRect = notif.GetComponent<RectTransform>();

        ach = GameObject.FindObjectOfType<AchieveMonitor>();
        if (ach == null)
        {
            ach = new AchieveMonitor();
        }

        DontDestroyOnLoad(this);
    }
    void OnEnable()
    {
        //check which have been completed
        UpdateAchievement(title: "Super Supporter", tierThresholds: superSupporterTiers, counterVar: ach.adClicks, achIndex: 0, descStart: "click on", descEnd: "ads", winDesc: "you've clicked all the ads!");
        UpdateAchievement(title: "Social Butterfly", tierThresholds: socialButterflyTiers, counterVar: ach.favorites, achIndex: 1, descStart: "favorite", descEnd: "player profiles", winDesc: "you're a real buttefly!");
        UpdateAchievement(title: "I Spy Master", tierThresholds: iSpyMasterTiers, counterVar: ach.minutes, achIndex: 1, descStart: "interact with the feed for", descEnd: "minutes", winDesc: "you're practically [INSERT COPYRIGHT FREE SPY NAME]!");
    }
    public void UpdateAchievement(int counterVar, int[] tierThresholds, int achIndex, string descStart, string descEnd, string winDesc, string title)
    {
        if (SceneManager.GetActiveScene().name == "Achievements")
        {
            if (counterVar >= tierThresholds[3] && tierThresholds[3] != -1)
            {
                AddStar(achievements[achIndex], 4, winDesc);
                tierThresholds[3] = -1;
            }
            else if (counterVar >= tierThresholds[2] && tierThresholds[2] != -1)
            {
                AddStar(achievements[achIndex], 3, $"{descStart} {tierThresholds[3]} {descEnd}");
                tierThresholds[2] = -1;
            }
            else if (counterVar >= tierThresholds[1] && tierThresholds[1] != -1)
            {
                AddStar(achievements[achIndex], 2, $"{descStart} {tierThresholds[2]} {descEnd}");
                tierThresholds[1] = -1;
            }
            else if (counterVar >= tierThresholds[0] && tierThresholds[0] != -1)
            {
                AddStar(achievements[achIndex], 1, $"{descStart} {tierThresholds[1]} {descEnd}");
                tierThresholds[0] = -1;
            }
        }
        else
        {
            if (counterVar >= tierThresholds[3] && tierThresholds[3] != -1)
            {
                StartCoroutine(notificationPopup($"You just achieved the final star of {title}!"));
                tierThresholds[3] = -1;
            }
            else if (counterVar >= tierThresholds[2] && tierThresholds[2] != -1)
            {
                StartCoroutine(notificationPopup($"You just achieved the third star of {title}!"));
                tierThresholds[2] = -1;
            }
            else if (counterVar >= tierThresholds[1] && tierThresholds[1] != -1)
            {
                StartCoroutine(notificationPopup($"You just achieved the second star of {title}!"));
                tierThresholds[1] = -1;
            }
            else if (counterVar >= tierThresholds[0] && tierThresholds[0] != -1)
            {
                StartCoroutine(notificationPopup($"You just achieved the first star of {title}!"));
                tierThresholds[0] = -1;
            }
        }  
    }

    public IEnumerator notificationPopup(string textToShow)
    {
        notifRect.anchoredPosition = new Vector2(0, Screen.height + 5);
        while (SceneManager.GetActiveScene().name == "LoginScene" && !SceneManager.GetActiveScene().isLoaded)
        {
            yield return new WaitForSeconds(1);
        }
        notifDesc.text = textToShow;
        yield return notifRect.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

        yield return new WaitForSeconds(3f);

        yield return notifRect.DOAnchorPosY(Screen.height + 5, 0.4f).SetEase(Ease.InQuad).WaitForCompletion();
    }
    public void GoToAchivements()
    {
        gm.ProgressToScene("Achievements");
        notif.transform.DOLocalMoveY(500, .3f).OnComplete(() => notif.SetActive(false));
    }
    public void AddStar(GameObject parentAch, int stars, string descMessage)
    {
        List<RawImage> starList = new();
        Transform starParent = parentAch.transform.GetChild(0);
        for (int i = 0; i < starParent.transform.childCount; i++)
        {
            starList.Add(starParent.transform.GetChild(i).GetComponentInChildren<RawImage>());
        }

        TextMeshProUGUI desc = parentAch.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        desc.text = descMessage;

        for (int i = 0; i < stars; i++)
        {
            Debug.Log(i);
            if (i % 3 == 0)
            {
                starList[i].color = blueCol;
            }
            else if (i % 2 == 0)
            {
                starList[i].color = pinkCol;
            }
            else
            {
                starList[i].color = redCol;
            }
        }
        
    }

    public void BackToLeaderBoard()
    {
        SceneManager.LoadScene("Achievements");
    }
}


