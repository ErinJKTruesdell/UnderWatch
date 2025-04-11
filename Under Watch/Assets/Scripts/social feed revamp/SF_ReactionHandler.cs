using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class SF_ReactionHandler : MonoBehaviour
{
    public SF_Cell cellScript;


    public Animator bannerSlide;
    public Animator emojiAnim;

    public GameObject[] emojiObjs;
    public Button[] emojiButtons;
    public IEnumerator Like(GameObject colorEmoji, Image banner, Image greyEmoji)
    {
        //if the button has already been clicked, unlike. otherwise, proceed
        if (colorEmoji.activeSelf)
        {
            StartCoroutine(Unlike(colorEmoji, banner, greyEmoji));
        }
        else
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

        yield return null;
    }

    public IEnumerator Unlike(GameObject colorEmoji, Image banner, Image greyEmoji)
    {
        bannerSlide.Play("bannerReverse");
        emojiAnim.Play("emojiReverse");

        greyEmoji.color = new(255, 255, 255, 255);

        yield return new WaitForSeconds(.15f);
        colorEmoji.SetActive(false);
        banner.color = new(0, 0, 0);

        yield return null;
    }
    public void LikeVoid(GameObject colorEmoji, Image banner, Image greyEmoji)
    {
        StartCoroutine(Like(colorEmoji, banner, greyEmoji));
    }
}
