using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using Mopsicus.InfiniteScroll;


public class PostUIHandling : MonoBehaviour
{    
    [SerializeField] public Image banner;
    [SerializeField] public Image bannerBorder;

    [SerializeField] Image greyEmoji;
    [SerializeField] GameObject colorEmoji;

    [SerializeField] GameObject parentItem;

    public Color32 blankCol;

    public Animator bannerSlide;
    public Animator emojiAnim;

    //may turn this into a List if the server interaction allows
    public static int smileLikes = 0;
    public static int thumbLikes = 0;
    public static int fireLikes = 0;
    public static int eyeLikes = 0;
    public static int gatorLikes = 0;

    public static string emojiClicked;
    void Start()
    {
        blankCol = new Color32(0, 0, 0, 0);

        colorEmoji.SetActive(false);
        banner.color = blankCol;
        bannerBorder.color = blankCol;
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

            if (parentItem.CompareTag("blue"))
            {
                bannerBorder.color = InfiniteScroll.pinkCol;
                banner.color = InfiniteScroll.redCol;
                colorEmoji.SetActive(true);
            }
            if (parentItem.CompareTag("pink"))
            {
                bannerBorder.color = InfiniteScroll.redCol;
                banner.color = InfiniteScroll.blueCol;
                colorEmoji.SetActive(true);
            }
            if (parentItem.CompareTag("red"))
            {
                bannerBorder.color = InfiniteScroll.blueCol;
                banner.color = InfiniteScroll.pinkCol;
                colorEmoji.SetActive(true);
            }

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
        if (name == "fireReact") { fireLikes += posNegNum; emojiClicked = "fire"; }
        if (name == "gatorReact") { gatorLikes += posNegNum; emojiClicked = "gator"; }
        if (name == "smileReact") { smileLikes += posNegNum; emojiClicked = "smile"; }
        if (name == "thumbReact") { thumbLikes += posNegNum; emojiClicked = "thumb"; }

        if (positive < 1)
        {
            //Unlike a post
            bannerBorder.color = blankCol;

            if (eyeLikes < 1) { UnLike(); }
            if (fireLikes < 1) { UnLike(); }
            if (gatorLikes < 1) { UnLike(); }
            if (smileLikes < 1) { UnLike(); }
            if (thumbLikes < 1) { UnLike(); }
        }

        Debug.Log("fire: " + fireLikes);

    }
    void UnLike()
    {
        banner.color = blankCol;
        colorEmoji.SetActive(false);
        greyEmoji.color = new Color32(255, 255, 255, 255);
    }
}
