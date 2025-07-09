using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;

public class ProfileStatManager : MonoBehaviour
{
    public TMP_Text objectivesText;
    public TMP_Text postsText;
    public TMP_Text reactsText;

    public TMP_Text workLoc;
    public TMP_Text attributes;

    public GameObject profileInfo;

    public GameManager gm;

    string un;
    private void OnEnable()
    {
        gm = FindObjectOfType<GameManager>();

        InitObjectivesCount();
        InitPostsAndReacts();
    }
    void InitPostsAndReacts()
    {
        if (SceneManager.GetActiveScene().name == ("ClickedProfile"))
        {
            un = ShowClickedProfile.userName;
        }
        else
        {
            un = GameManager.loggedInUser.un;
        }

        StartCoroutine(GetProfileData());
    }

    void InitObjectivesCount()
    {
        objectivesText.text = DayManager.objCompleted.ToString();
    }

    IEnumerator GetProfileData()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", un);
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

    public void HandleProfileStatDisplay(string response)
    {
        //$reactNum . "|" . $postNum. "|" . $work . "|" . $att1 . "|" . $att2 . "|" . $att3;
        try
        {
            profileInfo.SetActive(true);
            workLoc.text = "";
            attributes.text = "";

            string[] partition = response.Split("@");
            Debug.Log(partition[0]);
            Debug.Log(partition[1]);
            string[] statInfo = partition[0].Split("|");
            reactsText.text = statInfo[0];
            postsText.text = statInfo[1];

            string[] profAtt = partition[1].Split("|");

            Debug.Log("Assigning attributes" + profAtt[0] + profAtt[1] + profAtt[2] + profAtt[3]);

            //if the work string is empty, don't bother showing the rest
            //check if a string is empty before adding it to the sentence
            if (!string.IsNullOrEmpty(profAtt[0]))
                workLoc.text = "I work at " + profAtt[0];
            else
                throw new Exception("Attributes string is empty.");

            if (!string.IsNullOrEmpty(profAtt[1]))
                attributes.text += $" {profAtt[1]}.";

            if (!string.IsNullOrEmpty(profAtt[2]))
                attributes.text += $" {profAtt[2]}";

            if (!string.IsNullOrEmpty(profAtt[3]))
                attributes.text += $", and {profAtt[3]}!";
        }
        catch (Exception e)
        {
            Debug.Log(e);
            profileInfo.SetActive(false);
        }

        VLGFiddler.RebuildVLGLayout();
    }
}
