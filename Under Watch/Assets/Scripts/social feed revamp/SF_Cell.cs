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
    public string postID;

    //UI
    public TextMeshProUGUI unText;
    public TextMeshProUGUI locText;
    public TextMeshProUGUI postIDText;

    public RawImage postImage;
    public RawImage pfpImage;

    public Image colorFrame;

    //Model - is this taking up significant memory?
    public SFPostItem _postItem;
    private int _cellIndex;
    public SC_LoginSystem scls;

    bool hasLoadedPost = false;
    bool hasLoadedPfp = false;

    //ensure that these are added in order from smile -> gator
    public List<SF_ReactionEmoji> reacts = new();
    private List<string> allReactNames = new()
    {
        "smile",
        "thumb",
        "gator",
        "eye",
        "fire",
    };

    public enum CellColor
    {
        Red,
        Blue,
        Pink
    }
    public CellColor currentCellColor;

    private void Start()
    {
        scls = GameObject.FindObjectOfType<SC_LoginSystem>();

        //assign each react's value to the correct name\
        int i = 0;
        foreach (SF_ReactionEmoji react in reacts)
        {
            react.reactName = allReactNames[i];
            i++;
        }
    }

    //called every time this cell comes back into view. Consider moving image loading here to reduce memory impact
    public void ConfigureCell(SFPostItem postItem, int cellIndex)
    {
        SetCellColor(cellIndex);

        _cellIndex = cellIndex;
        _postItem = postItem;

        unText.text = postItem.username;
        locText.text = postItem.location;
        postIDText.text = postItem.postID;
        postID = postItem.postID;

        if (postItem.postPhoto == null || postItem.pfpPhoto == null)
        {
            StartCoroutine(LoadPostImages());
        }
        else
        {
            postImage.texture = postItem.postPhoto;
            pfpImage.texture = postItem.pfpPhoto;
        }

        _postItem.ReactNumDict = postItem.ReactNumDict;

        foreach (SF_ReactionEmoji react in reacts)
        {
            react.LoadReacts();
        }
    }
    IEnumerator LoadPostImages()
    {
        while (_postItem.postPhoto == null)
        {
            yield return new WaitForSeconds(.1f);
        }
        postImage.texture = _postItem.postPhoto;
        while (_postItem.pfpPhoto == null)
        {
            yield return new WaitForSeconds(.1f);
        }
        pfpImage.texture = _postItem.pfpPhoto;
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
