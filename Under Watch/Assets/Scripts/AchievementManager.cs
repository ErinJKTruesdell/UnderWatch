using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
public class AchievementObject
{
    public string title;
    public string description;
    public PointsManager.Source pointSource;
    public List<int> reqsPerLevel;
    public int progress;

    public List<int> pointsPerLevel = new List<int> { 25, 50, 75, 100 };
    public static readonly List<int> defaultPointsPerLevel = new() { 25, 50, 75, 100 };
    public int currentLevel;
    public AchievementObject(string _title, string _description, PointsManager.Source _pointSource, List<int> _reqsPerLevel, int _currentLevel = 0, int _progress = 0)
    {
        title = _title;
        description = _description;
        pointSource = _pointSource;

        reqsPerLevel = _reqsPerLevel;
        currentLevel = _currentLevel;
        progress = _progress;
    }
}

public class AchievementManager : MonoBehaviour
{
    public static List<AchievementObject> allAchievements;

    public GameObject achievementNotifObj;
    public Queue<IEnumerator> achievementQueue = new();
    public static bool isProcessingAchQueue;
    public Transform notifParent;
    void Awake()
    {
        DontDestroyOnLoad(this);
        AddAllAchievements();
    }

    public void UpdateAchievement(PointsManager.Source achType, int updateReqBy, bool resetCount)
    {
        //linq is a godless creation
        //find an achievement in the list if it's pointSource == achType
        var targetAch = allAchievements
            .FirstOrDefault(a => a.pointSource == achType);

        if (targetAch != null)
        {
            if (resetCount)
                targetAch.progress = updateReqBy;
            else if (!resetCount)
                targetAch.progress += updateReqBy;

            if (targetAch.currentLevel >= targetAch.reqsPerLevel.Count)
            {
                return;
            }
            else if (targetAch.progress >= targetAch.reqsPerLevel[targetAch.currentLevel])
            {
                Debug.Log("progress: " + targetAch.progress + " " + targetAch.currentLevel);
                //go to next level of target ach if we've surpassed the requirement, and if we're beneath the max reqs per level
                int pointsToGive = targetAch.pointsPerLevel[targetAch.currentLevel];

                PointsManager.AddPoints(GameManager.loggedInUser.un, pointsToGive, achType);

                EnqueueAchievement(targetAch.title, GetFormattedDescription(targetAch), pointsToGive);

                targetAch.currentLevel++;
            }
            AchievementEventHandler.InvokeUpdatedAchievement();
        }
    }
    void EnqueueAchievement(string title, string desc, int points)
    {
        achievementQueue.Enqueue(DisplayAchievement(title, desc, points));
        if (!isProcessingAchQueue && !ErrorManager.isProcessingErrorsQueue)
            StartCoroutine(ProcessAchievementQueue());
    }

    public IEnumerator ProcessAchievementQueue()
    {
        isProcessingAchQueue = true;
        while (achievementQueue.Count > 0)
        {
            IEnumerator nextAch = achievementQueue.Dequeue();
            yield return StartCoroutine(nextAch); 
        }
        isProcessingAchQueue = false;
    }

    public IEnumerator DisplayAchievement(string title, string desc, int points)
    {
        GameObject ach = Instantiate(achievementNotifObj, notifParent);
        AchievementNotification achNotif = ach.GetComponent<AchievementNotification>();
        yield return StartCoroutine(achNotif.ConfigureNotif(title, desc, points));
    }

    public void AddAllAchievements()
    {
        allAchievements = new()
        {
            new AchievementObject(
                _title: "Snap Streaker",
                _description: "Keep up your {value} day streak",
                _pointSource: PointsManager.Source.AchSnapStreaker,
                _reqsPerLevel: new List<int>
                {
                    2, 5, 8, 10
                }
            ),
            new AchievementObject(
                _title: "Gram Master",
                _description: "You’re a SnapGram pro at {value} minutes on the app!",
                _pointSource: PointsManager.Source.AchGramMaster,
                _reqsPerLevel: new List<int>
                {
                    10, 30, 60, 100
                }
            ),
            new AchievementObject(
                _title: "Early Worm",
                _description: "Starting your day right for the {value}{nth} time",
                _pointSource: PointsManager.Source.AchEarlyWorm,
                _reqsPerLevel: new List<int>
                {
                    1, 3, 6, 9
                }
            ),
            new AchievementObject(
                _title: "Bed Bug",
                _description: "Tucking you in for the {value}{nth} time",
                _pointSource: PointsManager.Source.AchBedBug,
                _reqsPerLevel: new List<int>
                {
                    1, 3, 6, 9
                }
            ),
            new AchievementObject(
                _title: "Social Butterfly",
                _description: "You just made {value} new friends with the power of selfies!",
                _pointSource: PointsManager.Source.AchSocialButterfly,
                _reqsPerLevel: new List<int>
                {
                    3, 4, 5, 6
                }
            ),
            new AchievementObject(
                _title: "Super Supporter",
                _description: "Thanks for supporting {value} sponsored brands!",
                _pointSource: PointsManager.Source.AchSuperSupporter,
                _reqsPerLevel: new List<int>
                {
                    5, 15, 30, 50
                }
            )
        };
        Debug.Log("Added all achievements to list, size: " + allAchievements.Count);
    }

    public static string GetFormattedDescription(AchievementObject _achObj)
    {
        List<int> reqNums = _achObj.reqsPerLevel;

        string descStr = _achObj.description;
        //by default just return the description

        //inserts the next req number wherever the {value} is found
        if (descStr.Contains("{value}"))
        {
            int replacer = _achObj.progress;
            descStr = descStr.Replace("{value}", replacer.ToString());

            if (descStr.Contains("{nth}"))
            {
                string replacerStr = ReturnNthString(replacer);
                descStr = descStr.Replace("{nth}", replacerStr);
            }
        }
        return descStr;
    }

    public static string ReturnNthString(int num)
    {
        //100% homegrown cage free code
        switch (num)
        {
            default:
                return "nth";
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            case 4 or 5 or 6 or 8:
                return "th";
            case int n when n == 7 || n >= 9:
                return "nth";
        }
    }

    private void OnEnable()
    {
        AchievementEventHandler.onIncrementedAchievement += UpdateAchievement;
    }
    private void OnDisable()
    {
        AchievementEventHandler.onIncrementedAchievement -= UpdateAchievement;
    }
}

