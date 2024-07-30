using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class ActivityStarter : MonoBehaviour
{
    
    AndroidJavaClass unityClass;
    AndroidJavaObject unityActivity;
    AndroidJavaClass customClass;
    public string username = "EJKTruesdell";

    void Start()
    {
        
    }

    public void setAlarms()
    {

        Permission.RequestUserPermission("android.permission.ACCESS_COARSE_LOCATION");
        Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");

        Permission.RequestUserPermission("android.permission.ACCESS_BACKGROUND_LOCATION");
        Permission.RequestUserPermission("android.permission.WAKE_LOCK");
        //Replace with your full package name
        sendActivityReference("com.example.alarmlibrary.AlarmServiceStarter");

        //Now, start service
        startService();
    }

    void sendActivityReference(string packageName)
    {
        unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        customClass = new AndroidJavaClass(packageName);
        object[] args = new object[] { unityActivity, username };
        customClass.CallStatic("receiveActivityInstance", args);
    }

    void startService()
    {
        customClass.CallStatic("StartCheckerService");
    }
}
