using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LeaderboardItem : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text pointsText;
    public RawImage profilePic;

    public void ClickOnProfile()
    {
        ShowClickedProfile.userName = usernameText.text;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }
}
