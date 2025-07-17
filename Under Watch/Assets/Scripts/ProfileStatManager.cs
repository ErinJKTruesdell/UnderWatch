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
    public TMP_Text pointText;
    public GameObject pointsPopUp;

    public GameObject profileInfo;

    public GameManager gm;
    public ProfileDatabase pd;

    string un;
    private void OnEnable()
    {
        gm = FindObjectOfType<GameManager>();

        InitPostsPointsAndReacts();
        InitObjectivesCount();
    }
    void InitPostsPointsAndReacts()
    {
        pointText.text = "Points: " + 0;
        if (SceneManager.GetActiveScene().name == "ClickedProfile")
        {
            un = ShowClickedProfile.user.un;
        }
        else
        {
            un = GameManager.loggedInUser.un;
        }

        PointsManager.GetUserPoints(un, GetPoints);
        StartCoroutine(GetProfileData());
    }

    void InitObjectivesCount()
    {
        objectivesText.text = DayManager.objCompleted.ToString();
    }

    void GetPoints(int points)
    {
        pointText.text = "Points: " + points.ToString();
        StartCoroutine(pointsAnim(points));
    }

    IEnumerator pointsAnim(int points)
    {
        int originalPoints = PlayerPrefs.GetInt("points");

        if (originalPoints != points)
        {
            pointsPopUp.SetActive(true);
            TextMeshProUGUI pointsText = pointsPopUp.GetComponent<TextMeshProUGUI>();
            pointsText.text = "+" + (points - originalPoints);
            StartCoroutine(FadeToTransparent(pointsText, 3));

            PlayerPrefs.SetInt("points", points);
            PlayerPrefs.Save();

            yield return new WaitForSeconds(3);

            pointsPopUp.SetActive(false);
        }
    }

    IEnumerator FadeToTransparent(TextMeshProUGUI text, float duration)
    {
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        text.color = endColor; // ensure it's fully transparent
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
                //$reactNum . "|" . $postNum. "|" . $work . "|" . $att1 . "|" . $att2 . "|" . $att3;
                HandleProfileStatDisplay(www.downloadHandler.text);
            }
        }
    }

    public void HandleProfileStatDisplay(string response)
    {
        try
        {
            //$work . "|" . $att1 . "|" . $att2 . "|" . $att3 ."@" . $reactNum . "|". $postNum . "|".
            profileInfo.SetActive(true);
            workLoc.text = "";
            attributes.text = "";

            string[] partition = response.Split("@");

            string[] statInfo = partition[1].Split("|");
            reactsText.text = statInfo[0];
            postsText.text = statInfo[1];

            string[] profAtt = partition[0].Split("|");

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

            VLGFiddler.RebuildVLGLayout();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            profileInfo.SetActive(false);
            VLGFiddler.RebuildVLGLayout();
        }
    }
}
