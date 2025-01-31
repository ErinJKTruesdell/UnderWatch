using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocialPostPopulate : MonoBehaviour, ICell
{

    public RawImage photoImg;
    public RawImage profImage;
    public TMP_Text unText;
    public TMP_Text locText;
    public TMP_Text postIDText;

    private ListItemInfo _listItem;

    private int _cellIndex;
    public void ConfigureCell(ListItemInfo listItem, int cellIndex)
    {
        _listItem = listItem;

        photoImg = listItem.photoImg;
        profImage = listItem.profImage;

        unText.text = listItem.unText;
        locText.text = listItem.locText;
        postIDText.text = listItem.postIDText;
    }
}
