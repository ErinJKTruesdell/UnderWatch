using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

    void Awake()
    {
        DontDestroyOnLoad(this);
        AddAllAchievements();
    }

    public void UpdateAchievement(PointsManager.Source achType, int updateReqBy)
    {
        //linq is a godless creation
        var targetAch = allAchievements
            .FirstOrDefault(a => a.pointSource == achType);

        if (targetAch != null)
        {
            targetAch.progress += updateReqBy;

            if (targetAch.progress >= targetAch.reqsPerLevel[targetAch.currentLevel] && targetAch.currentLevel < targetAch.reqsPerLevel.Count)
            {
                //go to next level of target ach if we've surpassed the requirement, and if we're beneath the max reqs per level
                targetAch.currentLevel++;
            }
        }
    }

    public List<AchievementObject> LoadListOfAchievements(List<AchievementObject> loadedAchievements)
    {
        //called when in ach scene:
        // check if loadedAchievements == allAchievements. If so, return
        bool isSameList = true;
        int i = 0;

        if (loadedAchievements == null)
            return allAchievements;
        foreach (AchievementObject ach in allAchievements)
            {
                if (ach.title != loadedAchievements[i].title)
                {
                    isSameList = false;
                    break;
                }
                i++;
            }

        if (isSameList)
            return null;
        else
            return allAchievements;
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

    private void OnEnable()
    {
        AchievementEventHandler.onIncrementedAchievement += UpdateAchievement;

    }
    private void OnDisable()
    {
        AchievementEventHandler.onIncrementedAchievement -= UpdateAchievement;
    }
}

