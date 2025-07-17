using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementEventHandler
{
    public delegate void AchievementAddToHandler(PointsManager.Source achType, int updateReqBy, bool resetCount = false);
    public delegate void AchievementUpdatedHandler();
    public static event AchievementAddToHandler onIncrementedAchievement;
    public static event AchievementUpdatedHandler onUpdatedAchievement;
    public static void InvokeAddToAchievment(PointsManager.Source achType, int updateReqBy, bool resetCount = false)
    {
        //methods in AchievementManager listen to this!
        onIncrementedAchievement?.Invoke(achType, updateReqBy, resetCount);
    }

    public static void InvokeUpdatedAchievement()
    {
        onUpdatedAchievement?.Invoke();
    }

    //AchievementEventHandler.InvokeAddToAchievment(PointsManager.Source.achievment, 1);
    //AchievementEventHandler.onCompletedAchievement += DisplayErrorMethod;

    //AchievementEventHandler.InvokeCompletedAchievement();
    //AchievementEventHandler.onIncrementedAchievement += DisplayErrorMethod;

}
