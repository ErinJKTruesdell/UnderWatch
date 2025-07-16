using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementEventHandler
{
    public delegate void CompletedReqHandler(int reqVal = 1, DayManager.ObjTypes type = DayManager.ObjTypes.other, int overrideVal = -1);
    public static event CompletedReqHandler OnCompletedAReq;
    public static event Action OnCompletedAllDayReqs;
    public static void InvokeAddToReq(int reqValue = 1, DayManager.ObjTypes reqName = DayManager.ObjTypes.other, int overrideValue = -1)
    {
        //most of the processing is done in DayManager.UpdateReq
        OnCompletedAReq?.Invoke(reqValue, reqName, overrideValue);
    }
    public static void InvokeAllReqsComplete()
    {
        OnCompletedAllDayReqs?.Invoke();
    }

    public static void LogSubscribers()
    {
        if (OnCompletedAReq == null)
        {
            Debug.Log("No subscribers to OnCompletedAReq.");
            return;
        }

        var subscribers = OnCompletedAReq.GetInvocationList();
        Debug.Log($"Total subscribers: {subscribers.Length}");

        foreach (var d in subscribers)
        {
            Debug.Log($"Method: {d.Method.Name}, Target: {d.Target}");
        }
    }

    //RequirementEventHandler.InvokeTaskCompleted("register", 1);
    //RequirementEventHandler.OnCompletedATask += UpdateReq;

}
