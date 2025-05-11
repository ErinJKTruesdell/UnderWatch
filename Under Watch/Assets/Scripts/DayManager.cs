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

    public Queue<string> achievementsQueue = new();
    bool isWorking = false;

    //refs
    public GameManager gm;
    public AchievementsManager achMan;
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
        UpdateReq("register new account", 1);
        UpdateReq("take profile picture", 1);
        UpdateReq("accept privacy policy", 1);
    }
    public void UpdateReq(string reqName, int value)
    {
        Tuple<int, int> reqData = currentDayReqs.requirements[reqName];

        if (currentDayReqs != null && currentDayReqs.requirements.ContainsKey(reqName))
        {
            //add the value int to the tracker number (item 1) of the req
            reqData = new Tuple<int, int> (reqData.Item1 + value, reqData.Item2);
            currentDayReqs.requirements[reqName] = reqData;
            Debug.Log("requirement " + reqName + "is at: " + reqData.Item1 + "/" + reqData.Item2);
        }
        if (reqData.Item1 >= reqData.Item2)
        {
            completedATask.Invoke();
            achievementsQueue.Enqueue(reqName);
            if (!isWorking)
                StartCoroutine(DequeueAchievements());

            Debug.Log("Requirement: " + reqName + " fulfilled!! Put a popup here");
        }
        CheckIfAllReqsFilled(currentDayReqs);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GoToNextDay();
        }
    }
    public void AllDayReqsFulfilled()
    {
        dayCompleted = true;
    }
    public void GoToEndDay()
    {
        dayCompleted = false;
        gm.ProgressToScene("EndOfDay");
    }
    public void GoToNextDay()
    {
        currentDay++;
        StartCoroutine(SendLevelData(currentDay));
        SetActiveReqs(currentDay);
        gm.ProgressToScene("SocialFeed");
    }
    public Dictionary<string, Tuple<int, int>> GetCurrentRequirements()
    {
        return currentDayReqs?.requirements;
    }
    public void AddAllRequirements()
    {
        //0 is tutorial
        AddNewRequirement(0, new List<(string, int, int)>()
        {
            ("register new account", 0, 1),
            ("take profile picture", 0, 1),
            ("accept privacy policy", 0, 1)
        });

        //ad frequency 0, snapgram announcement
        AddNewRequirement(1, new List<(string, int, int)>()
        {
            ("post 1 selfie", 0, 1),
            ("react to 3 posts", 0, 3),
            ("check announcement box", 0, 1)
        });

        AddNewRequirement(2, new List<(string, int, int)>()
        {
            ("post 1 selfie", 0, 5),
            ("total 3 favorited accounts", 0, 3),
            ("check engagement inbox", 0, 1)
        });

        //privacy policy update
        AddNewRequirement(3, new List<(string, int, int)>()
        {
            ("accept privacy policy", 0, 1),
            ("react to 5 posts", 0, 5),
            ("total 5 minutes app interaction", 0, 5)
        });

        //ad frequency 6, snapgram announcement
        AddNewRequirement(4, new List<(string, int, int)>()
        {
            ("post 2 selfies", 0, 2),
            ("post 1 selfie with another person", 0, 1),
            ("total 5 favorited accounts", 0, 5),
            ("check engagement inbox", 0 ,1)
        });

        AddNewRequirement(5, new List<(string, int, int)>()
        {
            ("post 2 selfies", 0, 2),
            ("post 1 selfie at location: Drexel Dragon", 0, 1),
            ("react to 10 posts", 0, 10),
            ("total 10 minutes app interaction", 0, 10),
            ("click 3 ads", 0, 3)
        });

        //ad frequency 5
        AddNewRequirement(6, new List<(string, int, int)>()
        {
            ("post 2 selfies", 0, 2),
            ("post 1 selfie with 2 other people", 0, 1),
            ("post 1 selfie from front angle", 0, 1),
            ("total 15 minutes app interaction", 0, 15),
            ("check engagement inbox", 0, 1)
        });

        //ad frequecy 4, whistleblower: SG data leak (is this worht it?)
        AddNewRequirement(7, new List<(string, int, int)>()
        {
            ("post 3 selifes", 0, 3),
            ("post 1 selfie at location: Lancaster Walk", 0, 1),
            ("post 1 selfie from front left angle", 0, 1),
            ("react to 15 posts", 0, 15),
            ("total 10 favorited accounts", 0, 10),
            ("click 5 ads", 0, 5)
        });

        //privacy policy update
        //ad frequecy 3, snapgram announcement
        AddNewRequirement(8, new List<(string, int, int)>()
        {
            ("accept privacy policy", 0, 1),
            ("post 3 selifes", 0, 3),
            ("post 1 selfie with 3 other people", 0, 1),
            ("post 1 selfie from front right angle", 0, 1),
            ("total 45 minutes app interaction", 0, 45),
            ("click 10 ads", 0, 10),
            ("check announcement box", 0, 1)
        });

        //ad frequency 2
        AddNewRequirement(9, new List<(string, int, int)>()
        {
            ("post 4 selfies", 0, 4),
            ("post 1 selfie at location: Your Favorite Food Cart", 0, 1),
            ("post 1 selfie w/ target user", 0, 1),
            ("post 1 selfie from left angle", 0, 1),
            ("react to 25 posts", 0, 25),
            ("total 15 favorited accounts", 0, 15),
            ("total 60 minutes app interaction", 0, 60),
            ("click 15 ads", 0, 15)
        });

        //ad frequency 1
        AddNewRequirement(10, new List<(string, int, int)>()
        {
            ("post 4 selfies", 0, 4),
            ("post 1 selfie with 4 other people", 0, 1),
            ("post 1 selfie at location: Billboard", 0, 1),
            ("post 1 selfie from right", 0, 1),
            ("react to 50 posts", 0, 50),
            ("total 100 minutes app interaction", 0, 100),
            ("click 20 ads", 0, 20)
        });
    }
    private void AddNewRequirement(int dayNum, List<(string, int, int)> reqData)
    {
        //fills a dict with all the requirements for one day
        Dictionary<string, Tuple<int, int>> reqDict = new();
        foreach (var req in reqData)
        {
            reqDict.Add(req.Item1, new Tuple<int, int>(req.Item2, req.Item3));
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
        foreach (KeyValuePair<string, Tuple<int, int>> kvp in currentReqs.requirements)
        {
            if (kvp.Value.Item1 != kvp.Value.Item2)
            {
                allReqsFilled = false;
                break;
            }
        }
        if (allReqsFilled)
            completedAllDayTasks.Invoke();
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
    IEnumerator DequeueAchievements()
    {
        isWorking = true;
        Debug.Log("count: " + achievementsQueue.Count);
        while (achievementsQueue.Count > 0)
        {
            string reqName = achievementsQueue.Dequeue();
            yield return StartCoroutine(achMan.notificationPopup(reqName + " completed!"));
        }
        isWorking = false;
    }
    IEnumerator SendLevelData(int currentLevel)
    {
        WWWForm form = new WWWForm();

        form.AddField("username", gm.scls.getUsername());
        form.AddField("level", currentLevel);

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
    public Dictionary<string, Tuple<int, int>> requirements;

    public DayRequirements(int day, Dictionary<string, Tuple<int, int>> reqs)
    {
        dayNum = day;
        requirements = reqs;
    }
}