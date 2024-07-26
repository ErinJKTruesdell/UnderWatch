using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mopsicus.InfiniteScroll;
using Unity.VisualScripting;


public class FrameColorChange : MonoBehaviour
{

    [SerializeField] Image frameImage;
    [SerializeField] Color32 frameBlue;
    [SerializeField] Color32 framePink;
    [SerializeField] Color32 frameRed;

    [SerializeField] public Sprite[] frameColors;
    //public bool testing;

    [SerializeField] InfiniteScroll infiniteScroll;

    private void Start()
    {
        infiniteScroll = GameObject.FindObjectOfType<InfiniteScroll>();

        //frameBlue = new Color32(99b, 202b, 225b);
        //framePink = new Color32(237f, 30f, 121f);
        //frameRed = new Color32(180f, 17f, 75f);
    }

    public void Update()
    {
        //if (infiniteScroll.pinkFrame) { ChangeImageColor(frameColors[0]); }
        //if (infiniteScroll.redFrame) { ChangeImageColor(frameColors[1]); }
    }

    public void ChangeImageColor(Sprite newImage)
    {

        gameObject.GetComponent<Image>().sprite = newImage;
        //gameObject.GetComponent<Image>().color = myColor;
    }

}
