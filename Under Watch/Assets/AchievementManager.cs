using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementObject
{
    public string title;
    public string description;
    public int currentLevel;
    public PointsManager.Source pointSource;
    public Dictionary<int, (int, int)> levelPointsReqsDict;
    public AchievementObject(string _title, string _description, int _currentLevel, PointsManager.Source _pointSource, Dictionary<int, (int points , int reqNumber)> _dict)
    {
        title = _title;
        description = _description;
        currentLevel = _currentLevel;
        pointSource = _pointSource;
        levelPointsReqsDict = _dict;
    }
}

public class AchievementManager : MonoBehaviour
{
    public string GetFormattedDescription(int level, string description, Dictionary<int, (int, int)> dict)
    {
        //inserts the next req number wherever the {value} is found
        if (dict.TryGetValue(level, out var requirements))
        {
            int needed = requirements.Item1;
            return description.Replace("{value}", needed.ToString());
        }
        return "Invalid level";
    }

    public void AddAchievementObject()
    {
        
    }

}

