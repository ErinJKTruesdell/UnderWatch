using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class PointsManager : MonoBehaviour
{
    public static PointsManager instance { get; private set; }
    
    public static int capturePoints = 100;
    public static int beCapturedPoints = 50;
    public static int targetReactedPoints = 2; 
    public static int postReactedPoints = 1; 
    public static int negChangeTargetPoints = -25; 
    public static int firstTierAchPoints = 25; 
    public static int secondTierAchPoints = 50; 
    public static int thirdTierAchPoints = 75; 
    public static int fourthTierAchPoints = 100; 

    public enum Source
    {
        TargetCapture,
        BeCaptured,
        PostReact,
        TargetPostReact,
        NegChangeTaret,
        AchSnapStreaker,
        AchGramMaster,
        AchEarlyWorm,
        AchBedBug,
        AchSocialButterfly,
        AchSuperSupporter,
        Testing,
        Other
    };

    //PointsManager.AddPoints(GameManager.loggedInUser.un, PointsManager.capturePoints, PointsManager.Source.TargetCapture);
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public static void AddPoints(string username, int points, Source source)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.AddPointsCoroutine(username, points, source));
        }
        else
        {
            Debug.LogError("PointsManager not initialized in scene!");
        }
    }

    public static void GetUserPoints(string username, Action<int> onComplete)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.GetUserPointsCoroutine(username, onComplete));
        }
        else
        {
            Debug.LogError("PointsManager not initialized in scene!");
        }
    }

    IEnumerator AddPointsCoroutine(string username, int points, Source source)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("points", points);
        form.AddField("source", source.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "add-points.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to add points: " + www.error + www.downloadHandler.text);
                ErrorEventHandler.InvokeError("Points Error", "Failed to add points: " + www.error, Color.red);
            }
            else if (www.downloadHandler.text.Contains("successfully"))
            {
                Debug.Log("Points added Successfully: " + points + " points to " + username);
            }
            else
            {
                ErrorEventHandler.InvokeError("Points Error", "Unknown error when adding points: " + www.downloadHandler.text, Color.red);
            }
        }
    }

    IEnumerator GetUserPointsCoroutine(string username, Action<int> onComplete)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-user-points.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to get points: " + www.error + www.downloadHandler.text);
                ErrorEventHandler.InvokeError("Points Error", "Failed to add points: " + www.error, Color.red);
            }
            else
            {
                string response = www.downloadHandler.text;

                if (response.Contains("succeed"))
                {
                    string[] parts = response.Split('|');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int totalPoints))
                    {
                        onComplete.Invoke(totalPoints);
                    }
                    else
                    {
                        Debug.LogError("Invalid response format: " + response);
                        onComplete.Invoke(0);
                    }
                }
                else
                {
                    Debug.LogError("Unexpected response: " + response);
                    onComplete.Invoke(0);
                }
            }
        }
    }

}
