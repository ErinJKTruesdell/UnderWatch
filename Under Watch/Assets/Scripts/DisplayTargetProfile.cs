using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DisplayTargetProfile : MonoBehaviour
{
public TMP_Text usernameText;
    public TMP_Text fullNameText;
    public RawImage profilePic;
    public RawImage zoomedProfilePic;

    public TMP_Text workLoc;
    public TMP_Text attributes;

    public GameObject profileInfo;

    public void ConfigureUser(UserInfo user)
    {
        usernameText.text = "@" + user.un;
        fullNameText.text = user.firstName + " " + user.lastName;
        profilePic.texture = user.profilePic;
        zoomedProfilePic.texture = user.profilePic;

        if (user.un != "")
        {
            StartCoroutine(GetProfileData(user.un));
        }
    }
    public void ClickOnProfile()
    {
        ShowClickedProfile.userName = usernameText.text;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }
    IEnumerator GetProfileData(string un)
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
                //$reactNum . "|" . $postNum. "|" . $work . "|" . $att1 . "|" . $att2 . "|" . $att3;
                HandleAttributeDisplay(www.downloadHandler.text);
            }
        }
    }

    void HandleAttributeDisplay(string response)
    {
        try
        {
            profileInfo.SetActive(true);

            workLoc.text = "";
            attributes.text = "";

            string[] partition = response.Split("@");
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
            workLoc.text = "";
            attributes.text = "";
        }
        VLGFiddler.RebuildVLGLayout();
    }
}
