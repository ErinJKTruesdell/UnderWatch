using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static OnlineMapsGPXObject;
using UnityEngine.Android;

public class GameManager : MonoBehaviour
{
    public static GameManager gmInstance;

    public static string rootURL = "egs01.westphal.drexel.edu/";

    public DateTime loginTime = new();
    public DateTime openSocialFeedTime = new();

    public SC_LoginSystem scls;
    public TouchScreenKeyboard keyboard;
    public AchieveMonitor ach;

    static public Color32 blueCol = new(99, 202, 225, 255);
    static public Color32 pinkCol = new(237, 30, 121, 255);
    static public Color32 redCol = new(180, 17, 75, 255);

    public UnityEvent userLoggedOut = new();

    public static UserInfo currTarget;
    public static UserInfo loggedInUser;
    private void Awake()
    {
        if (gmInstance == null)
        {
            gmInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        scls = GameObject.FindObjectOfType<SC_LoginSystem>();

        RegistrationManager.gm = this;
    }
    void Start()
    {
        if (scls == null)
        {
            scls = new SC_LoginSystem();
        }

        scls.gm = this;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        DontDestroyOnLoad(this);
    }
    public void ProgressToScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == "SocialFeed" && SceneManager.loadedSceneCount == 1)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    public void LogOut()
    {
        PlayerPrefs.DeleteKey("savedUsername");
        PlayerPrefs.DeleteKey("savedPassword");
        PlayerPrefs.DeleteKey("currentDay");
        PlayerPrefs.DeleteKey("objectivesCompleted");

        ProgressToScene("LoginScene");
        scls.isLoggedIn = false;
        loggedInUser = null;
        currTarget = null;

        userLoggedOut?.Invoke();

        Debug.Log("User login data cleared");
    }

    public void ForgotPassword()
    {
        ProgressToScene("ForgotPassword");
    }

# if UNITY_ANDROID
    private AndroidJavaObject GetCurrentActivity()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    private int GetSDKInt()
    {
        AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION");
        return version.GetStatic<int>("SDK_INT");
    }
#endif
}

public class UserInfo
{
    public Texture profilePic;
    public string un;
    public string firstName;
    public string lastName;
    public string email;
    //any other dating app info

    public UserInfo(string _un, string _firstName, string _lastName, Texture _profilePic = null, string _email = " ")
    {
        un = _un;
        firstName = _firstName;
        lastName = _lastName;

        profilePic = _profilePic;
        email = _email;

        Debug.Log("Saved user: " + un);
    }
}