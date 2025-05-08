using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DayManager : MonoBehaviour
{
    private Dictionary<int, DayRequirements> allDays = new();
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

        SetActiveReqs(currentDay);
    }

    private void Start()
    {
        AddAllRequirements();
    }
    public void SetActiveReqs(int dayNum)
    {
        //verify the day exists
        if (allDays.TryGetValue(dayNum, out var day))
        {
            currentDayReqs = day;
        }
    }

    public void FulFillReq(string reqName, bool value)
    {
        if (currentDayReqs != null && currentDayReqs.requirements.ContainsKey(reqName))
        {
            currentDayReqs.requirements[reqName] = value;
            completedATask.Invoke();
        }
        Debug.Log("Requirement: " + reqName + "fulfilled!! Put a popup here");

        CheckIfAllReqsFilled();
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
    public void GoToNextDay()
    {
        currentDay++;
        SetActiveReqs(currentDay);
        gm.ProgressToScene("SocialFeed");
    }

    private void AddNewRequirement(int day, string reqName)
    {
        allDays.Add(day, new DayRequirements(day, new Dictionary<string, bool>()
        {
            {reqName, false }
        }));
    }

    void CheckIfAllReqsFilled()
    {
        bool allReqsFilled = true;
        foreach (KeyValuePair<string, bool> kvp in currentDayReqs.requirements)
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
        AddNewRequirement(0, "register new account");
        AddNewRequirement(0, "take profile picture");
        AddNewRequirement(0, "accept privacy policy");

        //ad frequency 0, snapgram announcement
        AddNewRequirement(1, "post 1 selfie");
        AddNewRequirement(1, "react to 3 posts");
        AddNewRequirement(1, "check announcement box");

        AddNewRequirement(2, "post 1 selfie");
        AddNewRequirement(2, "total 3 favorited accounts");
        AddNewRequirement(2, "check engagement inbox");

        //privacy policy update
        AddNewRequirement(3, "accept privacy policy");
        AddNewRequirement(3, "react to 5 posts");
        AddNewRequirement(3, "total 5 minutes app interaction");

        //ad frequency 6, snapgram announcement
        AddNewRequirement(4, "post 2 selfies");
        AddNewRequirement(4, "post 1 selfie with another person");
        AddNewRequirement(4, "total 5 favorited accounts");
        AddNewRequirement(4, "check engagement inbox");

        AddNewRequirement(5, "post 2 selfies");
        AddNewRequirement(5, "post 1 selfie at location: Drexel Dragon");
        AddNewRequirement(5, "react to 10 posts");
        AddNewRequirement(5, "total 10 minutes app interaction");
        AddNewRequirement(5, "click 3 ads");

        //ad frequency 5
        AddNewRequirement(6, "post 2 selfies");
        AddNewRequirement(6, "post 1 selfie with 2 other people");
        AddNewRequirement(6, "post 1 selfie from front angle");
        AddNewRequirement(6, "total 15 minutes app interaction");
        AddNewRequirement(6, "check engagement inbox");

        //ad frequecy 4, whistleblower: SG data leak (is this worht it?)
        AddNewRequirement(7, "post 3 selifes");
        AddNewRequirement(7, "post 1 selfie at location: Lancaster Walk");
        AddNewRequirement(7, "post 1 selfie from front left angle");
        AddNewRequirement(7, "react to 15 posts");
        AddNewRequirement(7, "total 10 favorited accounts");
        AddNewRequirement(7, "click 5 ads");

        //privacy policy update
        //ad frequecy 3, snapgram announcement
        AddNewRequirement(8, "accept privacy policy");
        AddNewRequirement(8, "post 3 selifes");
        AddNewRequirement(8, "post 1 selfie with 3 other people");
        AddNewRequirement(8, "post 1 selfie from front right angle");
        AddNewRequirement(8, "total 45 minutes app interaction");
        AddNewRequirement(8, "click 10 ads");
        AddNewRequirement(8, "check announcement box");

        //ad frequency 2
        AddNewRequirement(9, "post 4 selfies");
        AddNewRequirement(9, "post 1 selfie at location: Your Favorite Food Cart");
        AddNewRequirement(9, "post 1 selfie w/ target user");
        AddNewRequirement(9, "post 1 selfie from front right angle");
        AddNewRequirement(9, "post 1 selfie from left angle");
        AddNewRequirement(9, "react to 25 posts");
        AddNewRequirement(9, "total 15 favorited accounts");
        AddNewRequirement(9, "total 60 minutes app interaction");
        AddNewRequirement(9, "click 15 ads");

        //ad frequency 1
        AddNewRequirement(10, "post 4 selfies");
        AddNewRequirement(10, "post 1 selfie with 4 other people");
        AddNewRequirement(10, "post 1 selfie at location: Billboard");
        AddNewRequirement(10, "post 1 selfie from right");
        AddNewRequirement(10, "react to 50 posts");
        AddNewRequirement(10, "total 100 minutes app interaction");
        AddNewRequirement(10, "click 20 ads");
        //end game
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