using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DayManager : MonoBehaviour
{
    public enum ObjTypes
    {
        other,
        privacyPolicy,
        selfie,
        selfieOthers,
        selfieLocation,
        selfieAngle,
        selfieTarget,
        react,
        announcements,
        minutes,
        adClicks,
        profilePic,
        register,
        engagementInbox,
        favorites,
    }

    public Dictionary<int, DayRequirements> allDays = new();
    public static DayRequirements currentDayReqs;

    public static bool dayCompleted = false;
    public static int currentDay { get; private set; }
    public static int maxDays { get; private set; }

    public Queue<string> achievementsQueue = new();
    bool isWorking = false;

    //refs
    public GameManager gm;
    public AchievementsManager achMan;

    public static List<string> reactedPostIDs = new();
    private void Awake()
    {
        DontDestroyOnLoad(this);
        gm = FindObjectOfType<GameManager>();

        AddAllRequirements();
        SetActiveReqs(currentDay);
    }
    private void Start()
    {
        StartCoroutine(SendLevelData(currentDay));
        //we have a day 0, so -1
        maxDays = allDays.Count -1;
    }
    public void UpdateReq(ObjTypes reqName, int value)
    {
        if (currentDayReqs != null && currentDayReqs.requirements.ContainsKey(reqName))
        {
            (string name, int progress, int total) reqData = currentDayReqs.requirements[reqName];

            //add the value int to the progress number (item 2) of the req
            reqData = (reqData.name, reqData.progress + value, reqData.total);
            currentDayReqs.requirements[reqName] = reqData;
            //Debug.Log("requirement " + reqName + "is at: " + reqData.Item1 + "/" + reqData.Item2);

            if (reqData.progress >= reqData.total)
            {
                //this requirement is fulfilled, popup and check if day is done
                achievementsQueue.Enqueue(reqData.name);
                if (!isWorking)
                    StartCoroutine(DequeueAchievements());

                Debug.Log("Requirement: " + reqName + " fulfilled!! Put a popup here");
                CheckIfAllReqsFilled(currentDayReqs);
                //this may be dangerous, but prevents multiple achievemetns in a row
                //currentDayReqs.requirements.Remove(reqName);
            }
        }
        else
            Debug.Log("currentDayReqs doesn't contain that key!");

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
    public void ProgressDay()
    {
        currentDay++;
        SetActiveReqs(currentDay);

        StartCoroutine(SendLevelData(currentDay));
    }
    public Dictionary<ObjTypes, (string name, int progress, int total)> GetCurrentRequirements()
    {
        return currentDayReqs?.requirements;
    }
    public void AddAllRequirements()
    {
        //0 is tutorial
        AddNewRequirement(0, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.register, "register new account", 0, 1), //
            (ObjTypes.profilePic, "take profile picture", 0, 1), //
            (ObjTypes.privacyPolicy, "accept privacy policy", 0, 1) //
        });

        //ad frequency 0, snapgram announcement
        AddNewRequirement(1, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 1 selfie", 0, 1), //
            (ObjTypes.react, "react to 3 posts", 0, 3), //
            (ObjTypes.announcements, "check announcement box", 0, 1), //
        });

        AddNewRequirement(2, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 1 selfie", 0, 1), //
           // (ObjTypes.favorites, "total 3 favorited accounts", 0, 3),
           // (ObjTypes.engagementInbox, "check engagement inbox", 0, 1)
        });

        //privacy policy update
        AddNewRequirement(3, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.privacyPolicy, "accept privacy policy", 0, 1), //
            (ObjTypes.react, "react to 5 posts", 0, 5), //
            (ObjTypes.minutes, "total 5 minutes app interaction", 0, 5) //
        });

        //ad frequency 6, snapgram announcement
        AddNewRequirement(4, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 2 selfies", 0, 2), //
            (ObjTypes.selfieOthers, "post 1 selfie with another person", 0, 1), //
           // (ObjTypes.favorites, "total 5 favorited accounts", 0, 5),
          //  (ObjTypes.engagementInbox, "check engagement inbox", 0 ,1)
        });

        AddNewRequirement(5, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 2 selfies", 0, 2), //
            (ObjTypes.selfieLocation, "post 1 selfie at location: Drexel Dragon", 0, 1), //
            (ObjTypes.react, "react to 10 posts", 0, 10), //
            (ObjTypes.minutes,"total 10 minutes app interaction", 0, 10), //
            (ObjTypes.adClicks, "click 3 ads", 0, 3) //
        });

        //ad frequency 5
        AddNewRequirement(6, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 2 selfies", 0, 2), //
            (ObjTypes.selfieOthers, "post 1 selfie with 2 other people", 0, 1), //
            (ObjTypes.selfieAngle, "post 1 selfie from front angle", 0, 1), //
            (ObjTypes.minutes, "total 15 minutes app interaction", 0, 15), //
           // (ObjTypes.engagementInbox, "check engagement inbox", 0, 1)
        });

        //ad frequecy 4, whistleblower: SG data leak (is this worht it?)
        AddNewRequirement(7, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 3 selifes", 0, 3), //
            (ObjTypes.selfieLocation, "post 1 selfie at location: Lancaster Walk", 0, 1),
            (ObjTypes.selfieAngle, "post 1 selfie from front left angle", 0, 1), //
            (ObjTypes.react, "react to 15 posts", 0, 15), //
        //    (ObjTypes.favorites, "total 10 favorited accounts", 0, 10),
            (ObjTypes.adClicks, "click 5 ads", 0, 5) //
        });

        //privacy policy update
        //ad frequecy 3, snapgram announcement
        AddNewRequirement(8, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.privacyPolicy, "accept privacy policy", 0, 1), //
            (ObjTypes.selfie, "post 3 selifes", 0, 3), //
            (ObjTypes.selfieOthers, "post 1 selfie with 3 other people", 0, 1), //
            (ObjTypes.selfieAngle, "post 1 selfie from front right angle", 0, 1), //
            (ObjTypes.minutes, "total 45 minutes app interaction", 0, 45), //
            (ObjTypes.adClicks, "click 10 ads", 0, 10), //
            (ObjTypes.announcements, "check announcement box", 0, 1) //
        });

        //ad frequency 2
        AddNewRequirement(9, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 4 selfies", 0, 4), //
            (ObjTypes.selfieLocation, "post 1 selfie at location: Your Favorite Food Cart", 0, 1), 
            (ObjTypes.selfieTarget, "post 1 selfie w/ target user", 0, 1), //
            (ObjTypes.selfieAngle, "post 1 selfie from left angle", 0, 1), //
            (ObjTypes.react, "react to 25 posts", 0, 25), //
           // (ObjTypes.favorites, "total 15 favorited accounts", 0, 15),
            (ObjTypes.minutes, "total 60 minutes app interaction", 0, 60), //
            (ObjTypes.adClicks, "click 15 ads", 0, 15) //
        });

        //ad frequency 1
        AddNewRequirement(10, new List<(ObjTypes, string, int, int)>()
        {
            (ObjTypes.selfie, "post 4 selfies", 0, 4), //
            (ObjTypes.selfieOthers, "post 1 selfie with 4 other people", 0, 1), //
            (ObjTypes.selfieLocation, "post 1 selfie at location: Billboard", 0, 1),
            (ObjTypes.selfieAngle, "post 1 selfie from right", 0, 1),
            (ObjTypes.react, "react to 50 posts", 0, 50), //
            (ObjTypes.minutes, "total 100 minutes app interaction", 0, 100), //
            (ObjTypes.adClicks, "click 20 ads", 0, 20) //
        });
    }

    void UpdateAdRate()
    {
        switch (currentDay)
        {
            case 0:
                //0
                break;
            case 4:
                //6
                break;
            case 6:
                
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
        }
    }
    private void AddNewRequirement(int dayNum, List<(ObjTypes, string, int, int)> reqData)
    {
        //fills a dict with all the requirements for one day
        Dictionary<ObjTypes, (string name, int progress, int total)> reqDict = new();
        foreach (var req in reqData)
        {
            reqDict.Add(req.Item1, (req.Item2, req.Item3, req.Item4));
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
        foreach (KeyValuePair<ObjTypes, (string name, int progress, int total)> kvp in currentReqs.requirements)
        {
            if (kvp.Value.progress != kvp.Value.total)
            {
                allReqsFilled = false;
                break;
            }
        }
        if (allReqsFilled)
            RequirementEventHandler.InvokeAllReqsComplete();
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
    public static bool DoesDayContainObjective(ObjTypes objective)
    {
        List<ObjTypes> currDayReqTypes = new(currentDayReqs.requirements.Keys);

        if (currDayReqTypes.Contains(objective))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static int RequirementProgress(ObjTypes reqName)
    {
        int reqProg = currentDayReqs.requirements[reqName].progress;
        return reqProg;
    }
    public static int RequirementTotal(ObjTypes reqName)
    {
        int reqProg = currentDayReqs.requirements[reqName].total;
        return reqProg;
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

    public IEnumerator setAdRate(int adrate)
    {
        // get data from server
        WWWForm form = new WWWForm();
        Debug.Log("Selected ad rate to upload: " + adrate);
        form.AddField("adRate", adrate.ToString());


        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "/set_ad_rate.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = www.error;
                Debug.Log(errorMessage);
            }
            else
            {
                //return null
                string responseText = www.downloadHandler.text;

                Debug.Log("Response: " + responseText);
            }
        }

    }
    private void OnEnable()
    {
        RequirementEventHandler.OnCompletedAllDayReqs += AllDayReqsFulfilled;
        RequirementEventHandler.OnCompletedAReq += UpdateReq;
    }
    private void OnDisable()
    {
        RequirementEventHandler.OnCompletedAllDayReqs -= AllDayReqsFulfilled;
        RequirementEventHandler.OnCompletedAReq -= UpdateReq;
    }
}

public class DayRequirements
{
    public int dayNum;

    public Dictionary<DayManager.ObjTypes, (string name, int progress, int total)> requirements;
    //type of objective - progress int, total int, display name
    public DayRequirements(int day, Dictionary<DayManager.ObjTypes, (string name, int progress, int total)> reqs)
    {
        dayNum = day;
        requirements = reqs;
    }
}