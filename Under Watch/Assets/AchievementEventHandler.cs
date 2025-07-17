using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementEventHandler
{
    public delegate void AchievementHandler(PointsManager.Source achType, int updateReqBy);
    public static event AchievementHandler onIncrementedAchievement;
    public static void InvokeAddToAchievment(PointsManager.Source achType, int updateReqBy)
    {
        //methods in AchievementManager listen to this!
        onIncrementedAchievement?.Invoke(achType, updateReqBy);
    }

    //AchievementEventHandler.InvokeAddToAchievment(PointsManager.Source.achievment, 1);
    //AchievementEventHandler.onIncrementedAchievement += DisplayErrorMethod;

}
