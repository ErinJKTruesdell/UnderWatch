using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using Mopsicus.InfiniteScroll;
using Unity.VisualScripting;
using UnityEngine.Networking;


public class PostUIHandling : MonoBehaviour
{
    public SocialFeedDatabase sfd;
    public SC_LoginSystem scls;

    [SerializeField] public Image banner;
    [SerializeField] public Image bannerBorder;
    [SerializeField] Image greyEmoji;

    public GameObject bannerObj;
    [SerializeField] GameObject colorEmoji;
    [SerializeField] GameObject parentItem;

    public Color32 blankCol;

    public Animator bannerSlide;
    public Animator emojiAnim;

    //may turn this into a List if the server interaction allows
    public int smileLikes;
    public int thumbLikes;
    public int fireLikes;
    public int eyeLikes;
    public int gatorLikes;
    //same order as the variables above
    public List<bool> isLikedByLoggedIn;

    public string emojiClicked;
    void Start()
    {
        blankCol = new Color32(0, 0, 0, 0);

        colorEmoji.SetActive(false);
        banner.color = blankCol;
        bannerBorder.color = blankCol;

        sfd = GameObject.FindObjectOfType<SocialFeedDatabase>();
        scls = GameObject.FindObjectOfType<SC_LoginSystem>();
    }

    public void OnPostLoad()
    {
        foreach (var item in isLikedByLoggedIn)
        {
            Debug.Log("occuring" + item.ToString());
        }

        if (smileLikes > 1) { ColorizeReacts(isLikedByLoggedIn[0]); }
        if (thumbLikes > 1) { ColorizeReacts(isLikedByLoggedIn[1]); }
        if (fireLikes > 1) { ColorizeReacts(isLikedByLoggedIn[2]); }
        if (eyeLikes > 1) { ColorizeReacts(isLikedByLoggedIn[3]); }
        if (gatorLikes > 1) { ColorizeReacts(isLikedByLoggedIn[4]); }
    }
    public void ChangeButtonImage()
    {
        if (greyEmoji.color == blankCol)
        {
            //remove a like, check if likes are zero
            AddLikes(0);
        }
        else
        {
            AddLikes(1);
            //set original image component to disabled and banner, color emoji enabled
            greyEmoji.color = blankCol;

            ColorizeReacts(true);
            //activate animations
            //gator requires a different scale than other emojis
            bannerSlide.Play("bannerAnim");
            if (colorEmoji.name == "gatorEmoji")
            {
                emojiAnim.Play("gatorPop");
            }
            else
            {
                emojiAnim.Play("emojiPop");
            }
        }
    }
    public void AddLikes(int positive)
    {
        //need to update the server's likes here
        int posNegNum = (1 - positive) * (-1) + (positive * 1);
       
        if (name == "eyeReact") { eyeLikes += posNegNum; emojiClicked = "eye"; }
        Debug.Log(eyeLikes);
        if (name == "fireReact") { fireLikes += posNegNum; emojiClicked = "fire"; }
        if (name == "gatorReact") { gatorLikes += posNegNum; emojiClicked = "gator"; }
        if (name == "smileReact") { smileLikes += posNegNum; emojiClicked = "smile"; }
        if (name == "thumbReact") { thumbLikes += posNegNum; emojiClicked = "thumb"; }
        StartCoroutine(SendLikes());

        if (positive < 1)
        {
            //Unlike a post
            bannerBorder.color = blankCol;

            if (eyeLikes < 1) { StartCoroutine(UnLike()); }
            if (fireLikes < 1) { StartCoroutine(UnLike()); }
            if (gatorLikes < 1) { StartCoroutine(UnLike()); }
            if (smileLikes < 1) { StartCoroutine(UnLike()); }
            if (thumbLikes < 1) { StartCoroutine(UnLike()); }
        }
    }
    IEnumerator UnLike()
    {
        bannerSlide.Play("bannerReverse");
        emojiAnim.Play("emojiReverse");
        greyEmoji.color = new Color32(255, 255, 255, 255);

        yield return new WaitForSeconds(.15f);
        colorEmoji.SetActive(false);

        banner.color = blankCol;
    }

    IEnumerator SendLikes()
    {
        WWWForm form = new WWWForm();

        form.AddField("username", scls.getUsername());
        form.AddField("react", emojiClicked);
        form.AddField("post_id", sfd.postID);

        //I dont think the like count is getting incremented

        using (UnityWebRequest www = UnityWebRequest.Post(sfd.rootURL + "toggle_reaction.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = www.error;
                Debug.Log(errorMessage);
                Debug.Log("React data send error, releasing queue");
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("react send: " + responseText);

            }
        }
    }

    public void ColorizeReacts(bool isLoggedInReact)
    {
        if (parentItem.CompareTag("blue"))
        {
            Debug.Log("blue");
            if (isLoggedInReact) { bannerBorder.color = InfiniteScroll.pinkCol; }
            banner.color = InfiniteScroll.redCol;
            colorEmoji.SetActive(true);
        }
        if (parentItem.CompareTag("pink"))
        {
            Debug.Log("pink");

            if (isLoggedInReact) { bannerBorder.color = InfiniteScroll.redCol; }
            banner.color = InfiniteScroll.blueCol;
            colorEmoji.SetActive(true);
        }
        if (parentItem.CompareTag("red"))
        {
            Debug.Log("red");

            if (isLoggedInReact) { bannerBorder.color = InfiniteScroll.blueCol; } 
            banner.color = InfiniteScroll.pinkCol;
            colorEmoji.SetActive(true);
        }
    }
}
