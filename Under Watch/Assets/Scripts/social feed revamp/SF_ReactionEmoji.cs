using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public Image banner;
    public Image border;
    public Image greyEmoji;

    private void Awake()
    {
        parentCell = GetComponentInParent<SF_Cell>();
        emojiAnim = colorEmoji.GetComponent<Animator>();
        banner.color *= 0;
        border.color *= 0;

        colorEmoji.SetActive(false);
    }

    IEnumerator Like()
    {
        //if the button has already been clicked
        if (colorEmoji.activeSelf)
        {
            StartCoroutine(Unlike());
        }
        else
        {
            //like!
            colorEmoji.SetActive(true);

            ColorizeReact();
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

        yield return null;
    }
    IEnumerator Unlike()
    {
        greyEmoji.color = Color.white;

        bannerSlide.Play("bannerReverse");
        emojiAnim.Play("emojiReverse");

        yield return new WaitForSeconds(.15f);
        colorEmoji.SetActive(false);

        banner.color *= 0;
        border.color *= 0;

        yield return null;
    }
    public void LikeVoid()
    {
        StartCoroutine(Like());
    }

    void ColorizeReact()
    {
        //color banner
        switch (parentCell.currentCellColor)
        {
            case CellColor.Red:
                banner.color = GameManager.pinkCol;
                border.color = GameManager.redCol;
                break;
            case CellColor.Pink:
                banner.color = GameManager.blueCol;
                border.color = GameManager.pinkCol;
                break;
            case CellColor.Blue:
                banner.color = GameManager.redCol;
                border.color = GameManager.blueCol;
                break;
        }
        //color border
    }
}
