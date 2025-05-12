using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RequirementEventHandler
{
    public static event Action<DayManager.ObjTypes, int> OnCompletedAReq;
    public static event Action OnCompletedAllDayReqs;
    public static void InvokeAddToReq(int reqValue, DayManager.ObjTypes reqName = DayManager.ObjTypes.other)
    {
        //most of the processing is done in DayManager.UpdateReq
        OnCompletedAReq?.Invoke(reqName, reqValue);
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
