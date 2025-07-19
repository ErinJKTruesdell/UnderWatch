using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class TimeSpentTracker : MonoBehaviour
{
    private float timeAccumulator = 0f;
    private int minutesReported = 0;
    void Update()
    {
        timeAccumulator += Time.deltaTime;

        if (timeAccumulator >= 60)
        {
            timeAccumulator -= 60;
            minutesReported++;
            StartCoroutine(SendMinutesToServer(1)); // Send 1 minute at a time
        }
    }

    public IEnumerator SendMinutesToServer(int minutes)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);
        form.AddField("minutes", minutes);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-ach-time.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to send time: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                Debug.Log("Time sent successfully: " + response + " min");

                int mins = Convert.ToInt32(response);
                if (mins > 0)
                {
                    AchievementManager.SetAchProgressObtained(PointsManager.Source.AchGramMaster, mins);
                    AchievementEventHandler.InvokeAddToAchievment(PointsManager.Source.AchGramMaster, mins, resetCount: true);
                }
            }
        }
    }
}
