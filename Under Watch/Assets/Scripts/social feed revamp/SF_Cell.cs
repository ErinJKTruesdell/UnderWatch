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
    public TextMeshProUGUI unText;
    public TextMeshProUGUI locText;
    public TextMeshProUGUI postIDText;

    public RawImage postImage;
    public RawImage pfpImage;

    public Image colorFrame;

    //Model - is this taking up significant memory?
    private SFPostItem _postItem;
    private int _cellIndex;
    public enum CellColor
    {
        Red,
        Blue,
        Pink
    }
    public CellColor currentCellColor;

    //called every time this cell comes back into view. Consider moving image loading here to reduce memory impact
    public void ConfigureCell(SFPostItem postItem, int cellIndex)
    {
        SetCellColor(cellIndex);

        _cellIndex = cellIndex;
        _postItem = postItem;

        unText.text = postItem.username;
        locText.text = postItem.location;
        postIDText.text = postItem.postID;

        if (postItem.postPhoto == null) Debug.Log("post null");
        if (postItem.pfpPhoto == null) Debug.Log("pfp null");

        postImage.texture = postItem.postPhoto;
        pfpImage.texture = postItem.pfpPhoto;
    }

    private void SetCellColor(int index)
    {
        index %= 3;
        switch (index)
        {
            case 1:
                colorFrame.color = GameManager.redCol;
                currentCellColor = CellColor.Red;
                break;
            case 2:
                colorFrame.color = GameManager.blueCol;
                currentCellColor = CellColor.Blue;
                break;
            case 0:
                colorFrame.color = GameManager.pinkCol;
                currentCellColor = CellColor.Pink;
                break;
        }
    }
    public void ClickOnProfile()
    {
        ShowClickedProfile.userName = unText.text;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }


}
