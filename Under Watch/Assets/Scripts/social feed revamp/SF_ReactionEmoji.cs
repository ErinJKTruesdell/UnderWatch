using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static SF_Cell;

public class SF_ReactionEmoji : MonoBehaviour
{
    //when post is blue, reaction is red
    //when post is red, reaction is pink
    //when post is pink, reaction is blue

    //outer is same color as border

    public SF_Cell parentCell;

    public Animator bannerSlide;
    public Animator emojiAnim;

    public GameObject colorEmoji;
    public GameObject bannerObj;
    public Image greyBanner;
    public Image banner;
    public Image border;
    public Image greyEmoji;

    public TextMeshProUGUI reactNumText;
    public int reactNum;
    bool userClicked = false;

    public Color neutralGrey = new(204, 204, 204, 255);
    public Color darkGrey;

    public string reactName = "";
    private void Awake()
    {
        parentCell = GetComponentInParent<SF_Cell>();
        emojiAnim = colorEmoji.GetComponent<Animator>();
    }

    //called when sf_cell's configuration finisihes
    public void LoadReacts()
    {
        //set the number text for the react
        reactNum = parentCell._postItem.ReactNumDict[reactName].Item1;
        reactNumText.text = reactNum.ToString();

        //if user has liked this reaction before
        if (parentCell._postItem.ReactNumDict[reactName].Item2 == true)
        {
            colorEmoji.SetActive(true);
            bannerObj.SetActive(true);
            userClicked = true;

            LikeSetup();
            ColorizeBanner();
        }
        //if this reaction has been liked by any user
        else if (reactNum > 0)
        {
            LikeSetup();
            greyBanner.color = neutralGrey;
            greyEmoji.color = Color.white;

            reactNumText.fontSize = 15;
            reactNumText.color = Color.white;

            colorEmoji.SetActive(false);
            bannerObj.SetActive(false);

        }
        else
        {
            ResetText();
            greyEmoji.color = Color.white;
            colorEmoji.SetActive(false);
            bannerObj.SetActive(false);
            greyBanner.color *= 0;
        }

    }

    IEnumerator Like()
    {
        //if the user has already clicked the button
        if (userClicked == true)
        {
            StartCoroutine(Unlike());
        }
        //if the user hasn't clicked before
        else
        {
            LikeSetup();
            FillLikeData();
            ColorizeBanner();
            LikeAnims();
        }
        yield return null;
    }
    void ResetText()
    {
        reactNumText.fontSize = 15;
        reactNumText.text = reactNum.ToString();
        reactNumText.color = darkGrey;
    }
    IEnumerator Unlike()
    {
        reactNum--;
        ResetText();
        userClicked = false;

        bannerSlide.Play("bannerReverse");
        emojiAnim.Play("emojiReverse");

        yield return new WaitForSeconds(.15f);
        //if other users have reacted
        if (reactNum > 0)
        {
            greyBanner.color = neutralGrey;
            reactNumText.color = Color.white;
            reactNumText.fontSize = 15;
        }
        else
        {
            greyBanner.color *= 0;
        }
        colorEmoji.SetActive(false);
        bannerObj.SetActive(false);

        //on the php server, if the like from user already is true, then it will toggle the like off.
        StartCoroutine(SendLikeDataToServer());
    }
    void FillLikeData()
    {
        userClicked = true;

        reactNum++;
        reactNumText.text = reactNum.ToString();

        StartCoroutine(SendLikeDataToServer());
    }
    void LikeSetup()
    {
        reactNumText.color = neutralGrey;
        reactNumText.fontSize = 20;
        colorEmoji.SetActive(true);
        bannerObj.SetActive(true);

    }
    void LikeAnims()
    {
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

    //this feels dangerous, could the user spam like/unlikes and crash the app?
    private IEnumerator SendLikeDataToServer()
    {
        WWWForm form = new WWWForm();

        form.AddField("username", parentCell.scls.getUsername());
        form.AddField("react", reactName);
        form.AddField("post_id", parentCell.postID);

        //I dont think the like count is getting incremented

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "toggle_reaction.php", form))
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
    public void LikeVoid()
    {
        StartCoroutine(Like());
    }

    void ColorizeBanner()
    {
        //color banner and border
        switch (parentCell.currentCellColor)
        {
            case CellColor.Red:
                banner.color = GameManager.pinkCol;
                break;
            case CellColor.Pink:
                banner.color = GameManager.blueCol;
                break;
            case CellColor.Blue:
                banner.color = GameManager.redCol;
                break;
        }
        reactNumText.color = Color.white;
        ColorizeBorder();
    }
    void ColorizeBorder()
    {
        //color banner and border
        switch (parentCell.currentCellColor)
        {
            case CellColor.Red:
                border.color = GameManager.redCol;
                break;
            case CellColor.Pink:
                border.color = GameManager.pinkCol;
                break;
            case CellColor.Blue:
                border.color = GameManager.blueCol;
                break;
        }
    }
}
