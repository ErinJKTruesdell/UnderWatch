using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;
using TMPro;
using UnityEngine.UI;
using System.Xml;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UIElements;

//The configuration of a cell is done through the DataSource SetCellData method.

public class SF_Cell : MonoBehaviour, ICell
{
    public string postID;
    public SC_LoginSystem scls;

    //UI
    public TextMeshProUGUI unText;
    public TextMeshProUGUI locText;
    public TextMeshProUGUI postIDText;
    public string adLink;

    public RawImage postImage;
    public RawImage pfpImage;

    public UnityEngine.UI.Image colorFrame;
    public Texture2D bgTex;
    public Texture2D snapPFPTex;

    //Model - is this taking up significant memory?
    public SFPostItem _postItem;
    private int _cellIndex;

    //ad handling
    public GameObject adButton;
    public UnityEngine.UI.Button pfpButton;

    List<int> loadedPosts = new();
    //ensure that these are added in order from smile -> gator
    public List<SF_ReactionEmoji> reacts = new();
    private List<string> allReactNames = new()
    {
        "smile",
        "thumb",
        "gator",
        "eye",
        "fire",
    };

    public enum CellColor
    {
        Red,
        Blue,
        Pink
    }
    public CellColor currentCellColor;

    private void Start()
    {
        scls = GameObject.FindObjectOfType<SC_LoginSystem>();

        //assign each react's value to the correct name\
        int i = 0;
        foreach (SF_ReactionEmoji react in reacts)
        {
            react.reactName = allReactNames[i];
            i++;
        }
    }

    //called every time this cell comes back into view. Consider moving image loading here to reduce memory impact
    public void ConfigureCell(SFPostItem postItem, int cellIndex)
    {
        SetCellColor(cellIndex);

        _cellIndex = cellIndex;
        _postItem = postItem;

        StartCoroutine(LoadPostID());
        StartCoroutine(LoadPostUN());
        StartCoroutine(LoadPostLocation());

        if (postItem.isAd)
        {
            pfpImage.texture = snapPFPTex;

            adButton.SetActive(true);
            locText.text = "Click to engage with Sponsor";

            StartCoroutine(LoadPostAdURL());
            StartCoroutine(LoadPostImage());

            if (!loadedPosts.Contains(cellIndex))
            {
                PopulateAdReacts(postItem);
                loadedPosts.Add(cellIndex);

                foreach (SF_ReactionEmoji react in reacts)
                {
                    react.isAd = true;
                }
            }
        }
        else
        {
            StartCoroutine(LoadPostImage());
            StartCoroutine(LoadPfpImage());
        }

        _postItem.ReactNumDict = postItem.ReactNumDict;
        foreach (SF_ReactionEmoji react in reacts)
        {
            react.LoadReacts();
        }
    }
    IEnumerator LoadPostID()
    {
        while (_postItem.postID == null)
            yield return new WaitForSeconds(.1f);
        postID = _postItem.postID;
        postIDText.text = postID;
    }
    IEnumerator LoadPostUN()
    {
        while (_postItem.posterUser == null || _postItem.posterUser.un == null)
            yield return new WaitForSeconds(0.1f);

        unText.text = _postItem.posterUser.un;
    }
    IEnumerator LoadPostLocation()
    {
        while (_postItem.placeName == null)
            yield return new WaitForSeconds(.1f);
        locText.text = _postItem.placeName;
        Debug.Log("Post location: " + _postItem.placeName);
        string coords = _postItem.latitude + ", " + _postItem.longitude;

        //hijacking the ad button 
        if (coords != "0, 0")
        {
            #if UNITY_IOS
                adLink = $"http://maps.apple.com/?daddr={coords}&dirflg=w";
                adButton.SetActive(true);
            #elif UNITY_ANDROID || UNITY_EDITOR
                adLink = $"https://www.google.com/maps/dir/?api=1&destination={coords}&travelmode=walking";
                adButton.SetActive(true);
            #else
                adLink = "";
                adButton.SetActive(false);       
            #endif
        }
    }
    IEnumerator LoadPostAdURL()
    {
        while (_postItem.posterUser.un == null)
            yield return new WaitForSeconds(.1f);

        adLink = _postItem.adLink;
    }

    IEnumerator LoadPostImage()
    {
        postImage.texture = bgTex;

        while (_postItem.postPhoto == null)
        {
            yield return new WaitForSeconds(.1f);
        }
        postImage.texture = _postItem.postPhoto;
    }
    IEnumerator LoadPfpImage()
    {
        pfpImage.texture = bgTex;

        while (_postItem.pfpPhoto == null)
        {
            yield return new WaitForSeconds(.1f);
        }
        pfpImage.texture = _postItem.pfpPhoto;

        _postItem.posterUser.profilePic = _postItem.pfpPhoto;
    }

    private void SetCellColor(int index)
    {
        index %= 3;
        switch (index)
        {
            case 1:
                colorFrame.color = GameManager.redCol;
                currentCellColor = CellColor.Red;
                break;
            case 2:
                colorFrame.color = GameManager.blueCol;
                currentCellColor = CellColor.Blue;
                break;
            case 0:
                colorFrame.color = GameManager.pinkCol;
                currentCellColor = CellColor.Pink;
                break;
        }
    }

    void PopulateAdReacts(SFPostItem postItem)
    {
        //{ "smile" , (0, false)},

        int i = 0;
        List<string> listOfKeys = new(postItem.ReactNumDict.Keys);

        foreach (string key in listOfKeys)
        {
            int likes = UnityEngine.Random.Range(10, 100);
            bool isUserLiked = false;

            postItem.ReactNumDict[key] = (likes, isUserLiked);
            i++;
        }
    }

    public void ClickOnProfile()
    {
        ShowClickedProfile.user = _postItem.posterUser;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }

    public void AdClick()
    {
        Debug.Log("ad clicked" + adLink);
        if (adLink != "")
        {
            Application.OpenURL(adLink);

            if (_postItem.isAd)
                AchievementEventHandler.InvokeAddToAchievment(PointsManager.Source.AchSuperSupporter, 1);
        }
    }
}
