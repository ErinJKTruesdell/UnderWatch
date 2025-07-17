using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementSpawner : MonoBehaviour
{
    public AchievementManager achievementManager;
    public GameObject achievementPrefab;

    void OEnable()
    {
        InstantiateAchievements();
    }

    void InstantiateAchievements()
    {
        foreach (AchievementObject ach in achievementManager.LoadAchievements())
        {

        }
    }
}
