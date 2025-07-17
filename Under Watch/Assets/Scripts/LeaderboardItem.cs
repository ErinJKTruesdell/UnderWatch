using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Coffee.UIEffects;

public class LeaderboardItem : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text pointsText;
    public RawImage profilePic;
    public UIEffect effects;
    public UserInfo user;

    public void ConfigreLI(UserInfo _user, int points)
    {
        user = _user;
        usernameText.text = _user.un;
        pointsText.text = points + " Points";

        if (_user.un == GameManager.loggedInUser.un)
        {
            //logged in user handling
            profilePic.texture = _user.profilePic;
            effects.enabled = true;
        }
        else
            effects.enabled = false;
            StartCoroutine(downloadImageFromURL(_user.profileURL, user));
    }

    public void ClickOnProfile()
    {
        ShowClickedProfile.user = user;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }

    IEnumerator downloadImageFromURL(string url, UserInfo user)
    {
        Debug.Log("Starting image Download Request for user: " + user.un + " from URL: " + url);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(GameManager.rootURL + url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            user.profilePic = ((DownloadHandlerTexture)request.downloadHandler).texture;
            profilePic.texture = user.profilePic;
        }
    }

}
