using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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

        AchievementEventHandler.onUpdatedAchievement += InstantiateAchievements;
    }
    void OnDisable()
    {
        AchievementEventHandler.onUpdatedAchievement -= InstantiateAchievements;
    }

    public void InstantiateAchievements()
    {
        List<AchievementObject> newAchs = AchievementManager.allAchievements;
        if (newAchs == null)
            return;
        else
        {
            ResetAchievements();

            foreach (AchievementObject ach in newAchs)
            {
                GameObject achObj = Instantiate(achievementPrefab, achievmentGrid);
                achObj.GetComponent<BadgeObject>().ConfigureAchievement(ach);
                loadedAchievements.Add(ach);
            }
        }
    }

    void ResetAchievements()
    {
        loadedAchievements = new();
        foreach (Transform child in achievmentGrid)
        {
            Destroy(child.gameObject);
        }
    }
}
