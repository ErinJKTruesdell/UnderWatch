using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;
using TMPro;
using UnityEngine.UI;
using System.Xml;
using UnityEngine.SceneManagement;

//The configuration of a cell is done through the DataSource SetCellData method.

public class SF_Cell : MonoBehaviour, ICell
{
    //UI
    public TMP_Text unText;
    public TMP_Text locText;
    public TMP_Text postIDText;

    public RawImage postImage;
    public RawImage pfpImage;

    //Model
    private SFPostItem _postItem;
    private int _cellIndex;

    public void ConfigureCell(SFPostItem postItem, int cellIndex)
    {
        _cellIndex = cellIndex;
        _postItem = postItem;

        unText.text = postItem.username;
        locText.text = postItem.location;
        postIDText.text = postItem.postID;

        postImage.texture = postItem.postPhoto;
        pfpImage.texture = postItem.pfpPhoto;
    }

    public void ClickOnProfile()
    {
        ShowClickedProfile.userName = unText.text;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }


}
