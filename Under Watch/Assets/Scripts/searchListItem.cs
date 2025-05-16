using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class searchListItem : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text fullNameText;
    public RawImage profilePic;
    public void ClickOnProfile()
    {
        PlayerPrefs.SetString("SearchedUNs", SearchScript.prevSearches += usernameText.text.ToLower());
        PlayerPrefs.Save();

        ShowClickedProfile.userName = transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }

}
