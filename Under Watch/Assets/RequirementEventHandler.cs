using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RequirementEventHandler
{
    public static event Action<string, int> OnCompletedAReq;
    public static event Action OnCompletedAllDayReqs;
    public static void InvokeReqCompleted(string reqName, int reqValue)
    {
        OnCompletedAReq?.Invoke(reqName, reqValue);
    }
    public static void InvokeAllReqsComplete()
    {
        OnCompletedAllDayReqs?.Invoke();
    }
    //RequirementEventHandler.InvokeTaskCompleted("register", 1);
    //RequirementEventHandler.OnCompletedATask += UpdateReq;


}
