using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
public class LeaderboardSpawner : MonoBehaviour
{
    List<(UserInfo, int)> formattedUsers = new();
    public List<LeaderboardItem> leaderBoardSlots = new();

    void OnEnable()
    {
        StartCoroutine(GetTopUsers());
    }
    IEnumerator GetTopUsers()
    {
        WWWForm form = new WWWForm();

        form.AddField("username", GameManager.loggedInUser.un);

        UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-leaderboard.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string result = www.downloadHandler.text;
            Debug.Log("Raw result: " + result);

            // Split the result into user strings
            string[] userEntries = result.Split('@');

            formattedUsers.Clear();

            foreach (string entry in userEntries)
            {
                if (!string.IsNullOrWhiteSpace(entry))
                {
                    string[] parts = entry.Split('|');
                    if (parts.Length == 5)
                    {
                        string username = parts[0];
                        string firstName = parts[1];
                        string lastName = parts[2];
                        int points = Convert.ToInt32(parts[3]);
                        string url = parts[4];

                        string formatted = $"{firstName} {lastName} (@{username}) (@{url})  has {points} points.";
                        Debug.Log(formatted);

                        UserInfo user = new UserInfo(username, firstName, lastName, _profileURL: url);

                        formattedUsers.Add((user, points));
                        PointsManager.GetUserPoints(GameManager.loggedInUser.un, SpawnUsers);
                    }
                    else
                    {
                        Debug.Log("not enough parts!");
                        ErrorEventHandler.InvokeError("Server Response Error", "Wrong format of leaderboard response", Color.red);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Request failed: " + www.error);
        }
    }

    void SpawnUsers(int loggedInUserPoints)
    {
        Debug.Log("spawning users!");

    //check if user is already in top 3
        bool userIsTop3 = false;
        foreach ((UserInfo, int) user in formattedUsers)
        {
            if (user.Item1.un == GameManager.loggedInUser.un)
            {
                userIsTop3 = true;
                break;
            }
        }
        if (!userIsTop3)
            formattedUsers.Add((GameManager.loggedInUser, loggedInUserPoints));

        for (int i = 0; i < formattedUsers.Count; i++)
        {
            leaderBoardSlots[i].gameObject.SetActive(true);

            leaderBoardSlots[i].ConfigreLI(formattedUsers[i].Item1, formattedUsers[i].Item2);
        }
        VLGFiddler.RebuildVLGLayout();
    }
}
