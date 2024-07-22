using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class PostUIHandling : MonoBehaviour
{
    [SerializeField] Sprite newButtonImage;
    [SerializeField] Button button;
    public void ChangeButtonImage()
    {
        button.image.sprite = newButtonImage;
    }
}
