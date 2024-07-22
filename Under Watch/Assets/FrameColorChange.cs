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
    [SerializeField] public Color frameBlue;
    [SerializeField] public Color framePink;
    [SerializeField] public Color frameMagenta;

    private void Start()
    {


        frameBlue = new Color(0f, 150f, 15f);

        gameObject.GetComponent<Image>().color = frameBlue;
    }

    public void ChangeImageColor(int num)
    {
        Debug.Log(num);

        if (num % 3 == 0)
        {
            frameImage.color = frameBlue;
        }
        else if (num % 2 == 0)
        {
            frameImage.color = framePink;
        }
        else
        {
            frameImage.color = frameMagenta;
        }

    }

}
