using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DayManager : MonoBehaviour
{
    public Dictionary<int, DayRequirements> allDays = new();
    private DayRequirements currentDayReqs;

    public static bool dayCompleted = false;
    public static int currentDay { get; private set; }
    public UnityEvent completedAllDayTasks;
    public UnityEvent completedATask;

    //refs
    public GameManager gm;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        gm = FindObjectOfType<GameManager>();

        completedAllDayTasks.AddListener(AllDayReqsFulfilled);
        AddAllRequirements();
        SetActiveReqs(currentDay);
    }

    private void Start()
    {
        StartCoroutine(SendLevelData(currentDay));
        FulFillReq("register new account", true);
    }
    public void SetActiveReqs(int dayNum)
    {
        //verify the day exists
        if (allDays.TryGetValue(dayNum, out var day))
        {
            currentDayReqs = day;
        }
        else
        {
            Debug.Log("Day does not exist!");
        }
    }

    public void FulFillReq(string reqName, bool value)
    {
        if (currentDayReqs != null && currentDayReqs.requirements.ContainsKey(reqName))
        {
            currentDayReqs.requirements[reqName] = value;
            completedATask.Invoke();
        }
        Debug.Log("Requirement: " + reqName + " fulfilled!! Put a popup here");

        CheckIfAllReqsFilled(currentDayReqs);
    }

    public void AllDayReqsFulfilled()
    {
        dayCompleted = true;
    }
    public void GoToEndDay()
    {
        dayCompleted = false;
        gm.ProgressToScene("SocialFeed");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GoToNextDay();
        }
    }
    public void GoToNextDay()
    {
        currentDay++;
        StartCoroutine(SendLevelData(currentDay));
        SetActiveReqs(currentDay);
        gm.ProgressToScene("SocialFeed");
    }

    private void AddNewRequirement(int dayNum, List<string> reqNames)
    {
        //fills a dict with all the requirements for one day
        Dictionary<string, bool> reqDict = new();
        foreach (string req in reqNames)
        {
            reqDict.Add(req, false);
        }

        //adds a new DayReq object, with the day's requirements and number and adds that to AddToDay
        AddToDay(dayNum, new DayRequirements(dayNum, reqDict));
    }
    private void AddToDay(int dayNum, DayRequirements DayReqs)
    {
        allDays.Add(dayNum, DayReqs);
    }

    void CheckIfAllReqsFilled(DayRequirements currentReqs)
    {
        bool allReqsFilled = true;
        Debug.Log(currentReqs.requirements);
        foreach (KeyValuePair<string, bool> kvp in currentReqs.requirements)
        {
            if (kvp.Value == false)
            {
                allReqsFilled = false;
                break;
            }
        }
        if (allReqsFilled)
            AllDayReqsFulfilled();
    }
    public Dictionary<string, bool> GetCurrentRequirements()
    {
        return currentDayReqs?.requirements;
    }

    public void AddAllRequirements()
    {
        //0 is tutorial
        AddNewRequirement(0, new List<string>()
        {
            "register new account",
            "take profile picture",
            "accept privacy policy"
        });

        //ad frequency 0, snapgram announcement
        AddNewRequirement(1, new List<string>()
        {
            "post 1 selfie",
            "react to 3 posts",
            "check announcement box"
        });

        AddNewRequirement(2, new List<string>()
        {
            "post 1 selfie",
            "total 3 favorited accounts",
            "check engagement inbox",
        });

        //privacy policy update
        AddNewRequirement(3, new List<string>()
        {
            "accept privacy policy",
            "react to 5 posts",
            "total 5 minutes app interaction"
        });

        //ad frequency 6, snapgram announcement
        AddNewRequirement(4, new List<string>()
        {
            "post 2 selfies",
            "post 1 selfie with another person",
            "total 5 favorited accounts",
            "check engagement inbox"
        });

        AddNewRequirement(5, new List<string>()
        {
            "post 2 selfies",
            "post 1 selfie at location: Drexel Dragon",
            "react to 10 posts",
            "total 10 minutes app interaction",
            "click 3 ads"
        });

        //ad frequency 5
        AddNewRequirement(6, new List<string>()
        {
            "post 2 selfies",
            "post 1 selfie with 2 other people",
            "post 1 selfie from front angle",
            "total 15 minutes app interaction",
            "check engagement inbox"
        });

        //ad frequecy 4, whistleblower: SG data leak (is this worht it?)
        AddNewRequirement(7, new List<string>()
        {
            "post 3 selifes",
            "post 1 selfie at location: Lancaster Walk",
            "post 1 selfie from front left angle",
            "react to 15 posts",
            "total 10 favorited accounts",
            "click 5 ads"
        });

        //privacy policy update
        //ad frequecy 3, snapgram announcement
        AddNewRequirement(8, new List<string>()
        {
            "accept privacy policy",
            "post 3 selifes",
            "post 1 selfie with 3 other people",
            "post 1 selfie from front right angle",
            "total 45 minutes app interaction",
            "click 10 ads",
            "check announcement box"
        });

        //ad frequency 2
        AddNewRequirement(9, new List<string>()
        {
            "post 4 selfies",
            "post 1 selfie at location: Your Favorite Food Cart",
            "post 1 selfie w/ target user",
            "post 1 selfie from left angle",
            "react to 25 posts",
            "total 15 favorited accounts",
            "total 60 minutes app interaction",
            "click 15 ads"
        });

        //ad frequency 1
        AddNewRequirement(10, new List<string>()
        {
            "post 4 selfies",
            "post 1 selfie with 4 other people",
            "post 1 selfie at location: Billboard",
            "post 1 selfie from right",
            "react to 50 posts",
            "total 100 minutes app interaction",
            "click 20 ads"
        });
    }

    IEnumerator SendLevelData(int currentLevel)
    {
        WWWForm form = new WWWForm();

        form.AddField("username", gm.scls.getUsername());
        form.AddField("level", currentLevel);

        //I dont think the like count is getting incremented

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "level_update.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = www.error;
                Debug.Log(errorMessage);
                Debug.Log("level data send error, releasing queue");
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("level send: " + responseText);
            }
        }

    }
}

public class DayRequirements
{
    public int dayNum;
    public Dictionary<string, bool> requirements;

    public DayRequirements(int day, Dictionary<string, bool> reqs)
    {
        dayNum = day;
        requirements = reqs;
    }
}