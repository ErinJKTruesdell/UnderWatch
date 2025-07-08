using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System;

public class ProfileStatManager : MonoBehaviour
{
    public TMP_Text objectivesText;
    public TMP_Text postsText;
    public TMP_Text reactsText;

    public TMP_Text workLoc;
    public TMP_Text attributes;

    public GameObject profileInfo;

    public GameManager gm;

    private void OnEnable()
    {
        gm = FindObjectOfType<GameManager>();

        InitObjectivesCount();
        InitPostsAndReacts();
        profileInfo.SetActive(true);

    }

    void InitPostsAndReacts()
    {
        StartCoroutine(GetProfileData());
    }

    void InitObjectivesCount()
    {
        objectivesText.text = DayManager.objCompleted.ToString();
    }

    IEnumerator GetProfileData()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);
        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-profile-stats.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to load level: " + www.error + www.downloadHandler.text);
                yield break;
            }
            else
            {
                Debug.Log("response: " + www.downloadHandler.text);

                HandleProfileStatDisplay(www.downloadHandler.text);
            }
        }
    }

    void HandleProfileStatDisplay(string response)
    {
        //$reactNum . "|" . $postNum. "|" . $work . "|" . $att1 . "|" . $att2 . "|" . $att3;
        try
        {
            string[] partition = response.Split("@");
            string[] statInfo = partition[0].Split("|");
            string[] profAtt = partition[1].Split("|");

            reactsText.text = statInfo[0];
            postsText.text = statInfo[1];

            //if all the strings are empty, throw an exception
            if (string.IsNullOrEmpty(profAtt.ToString()))
                throw new Exception("Attributes string is empty.");

            //check if a string is empty before adding it to the sentence
            if (!string.IsNullOrEmpty(profAtt[2]))
                workLoc.text = "I work at " + profAtt[2];

            if (!string.IsNullOrEmpty(profAtt[3]))
                attributes.text += $" {profAtt[3]}.";

            if (!string.IsNullOrEmpty(profAtt[4]))
                attributes.text += $" {profAtt[4]}";

            if (!string.IsNullOrEmpty(profAtt[5]))
                attributes.text += $", and {profAtt[5]}!";
        }
        catch (Exception e)
        {
            Debug.Log(e);
            profileInfo.SetActive(false);
        }
    }
}
