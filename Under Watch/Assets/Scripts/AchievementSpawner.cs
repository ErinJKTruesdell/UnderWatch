using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementSpawner : MonoBehaviour
{
    public AchievementManager achievementManager;
    public GameObject achievementPrefab;
    public Transform achievmentGrid;
    public List<AchievementObject> loadedAchievements;
    void OnEnable()
    {
        if (achievementManager == null)
            achievementManager = FindObjectOfType<AchievementManager>();

        InstantiateAchievements();

        AchievementEventHandler.InvokeAddToAchievment(PointsManager.Source.AchSnapStreaker, 1);
    }

    void InstantiateAchievements()
    {
        List<AchievementObject> newAchs = achievementManager.LoadListOfAchievements(loadedAchievements);
        if (newAchs == null)
            return;
        else
        {
            loadedAchievements = new();

            foreach (AchievementObject ach in newAchs)
            {
                GameObject achObj = Instantiate(achievementPrefab, achievmentGrid);
                achObj.GetComponent<BadgeObject>().ConfigureAchievement(ach);
                loadedAchievements.Add(ach);
            }
        }
    }
}
