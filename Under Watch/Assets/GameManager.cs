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

    private float sessionTimer = 0f;

    public UnityEvent userLoggedOut = new();
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
    public void saveLoginTime()
    {
        loginTime = DateTime.Now;

        Debug.Log("Starting app time tracking...");
    }

    public void saveSocialFeedTime()
    {
        Debug.Log("Social Feed Opened");
        openSocialFeedTime = DateTime.Now;
    }

    public void onSocialFeedClosed()
    {
        Debug.Log("Social Feed Closed");
        //upload social feed
        if(scls.getUsername() != null)
        {
            double timeinMinutes = Math.Round((DateTime.Now - openSocialFeedTime).TotalMinutes, 2);
            StartCoroutine(sendSocialTimeToDatabase(timeinMinutes, scls.getUsername()));
        }

    }

    void saveAppTime() //called on pause and on quit
    {
        if (scls.getUsername() != null)
        {
            double timeinMinutes = Math.Round((DateTime.Now - loginTime).TotalMinutes, 2);
            if (SceneManager.GetActiveScene().name == "SocialFeed")
            {
                onSocialFeedClosed();
            }
            StartCoroutine(sendAppTimeToDatabase(timeinMinutes, scls.getUsername()));
        }
    }

    private void Update()
    {
        RequirementTimeTracker();
    }
    public void RequirementTimeTracker()
    {
        sessionTimer += Time.deltaTime;
        //saves every minute
        if (sessionTimer >= 60f)
        {
            float totalTime = PlayerPrefs.GetFloat("TotalTimePlayed", 0f);
            totalTime += sessionTimer;

            int intTime = Mathf.RoundToInt(totalTime) / 60;
            //set the override value to the current timer
            RequirementEventHandler.InvokeAddToReq(0,DayManager.ObjTypes.minutes, intTime);

            PlayerPrefs.SetFloat("TotalTimePlayed", totalTime);
            PlayerPrefs.Save();
            sessionTimer -= 60f;
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            saveAppTime();
        }
        else
        {
            loginTime = DateTime.Now;
        }
    }

    private void OnApplicationQuit()
    {
        saveAppTime();
    }

    public void ProgressToScene(string sceneName)
    {

        if(SceneManager.GetActiveScene().name == "SocialFeed" &&  SceneManager.loadedSceneCount == 1)
        {
            onSocialFeedClosed();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        if (sceneName == "SocialFeed")
        {
            saveSocialFeedTime();
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
        scls.userName = "";
        scls.userEmail = "";

        userLoggedOut?.Invoke();

        Debug.Log("User login data cleared");
    }

    public void ForgotPassword()
    {
        ProgressToScene("ForgotPassword");
    }

    IEnumerator sendSocialTimeToDatabase(double minutes, string username)
    {
        ach.addMinutes(minutes);

        Debug.Log("Social feed closed by " + username + " after " + minutes + " minutes.");
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("time", minutes.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "append-social-feed-time.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Non-Success Result");
                string responseText = www.downloadHandler.text;

                Debug.Log(responseText);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                
                    Debug.Log(responseText);            
            }
        }
    }

    IEnumerator sendAppTimeToDatabase(double minutes, string username)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("time", minutes.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "append-in-app-time.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Non-Success Result");
            }
            else
            {
                string responseText = www.downloadHandler.text;

                Debug.Log(responseText);

            }
        }
    }
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


}
