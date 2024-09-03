using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static OnlineMapsGPXObject;

public class GameManager : MonoBehaviour
{

    string rootURL = "egs01.westphal.drexel.edu/";

    public DateTime loginTime;
    public DateTime openSocialFeedTime;


    public SC_LoginSystem scls;
    public TouchScreenKeyboard keyboard;
    // Start is called before the first frame update
    void Start()
    {
        scls = GameObject.FindObjectOfType<SC_LoginSystem>();
        if (scls == null)
        {
            scls = new SC_LoginSystem();
        }

        scls.gm = this;

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

        if(SceneManager.GetActiveScene().name == "SocialFeed")
        {
            onSocialFeedClosed();
        }
        if(sceneName == "SocialFeed")
        {
            saveSocialFeedTime();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ForgotPassword()
    {
        ProgressToScene("ForgotPassword");
    }

    IEnumerator sendSocialTimeToDatabase(double minutes, string username)
    {
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
}
