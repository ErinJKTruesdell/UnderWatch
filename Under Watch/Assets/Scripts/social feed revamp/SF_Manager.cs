using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;
public struct SFPostItem
{
    public string username;
    public string location;
    public string postID;

    public Texture postPhoto;
    public Texture pfpPhoto;
}

public class SF_Manager : MonoBehaviour, IRecyclableScrollRectDataSource
{
    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    [SerializeField]
    private int _dataLength;

    private List<SFPostItem> postList = new();

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        InitData();
        _recyclableScrollRect.DataSource = this;
    }
    //Initialising postList with dummy data 
    private void InitData()
    {
        if (postList != null) postList.Clear();

        string[] locs = { "here", "not here" };
        for (int i = 0; i < _dataLength; i++)
        {
            SFPostItem obj = new SFPostItem();
            obj.username = i + "_Name";
            obj.location = locs[Random.Range(0, 2)];
            obj.postID = "item : " + i;
            postList.Add(obj);
        }
    }

    #region DATA-SOURCE

    /// <summary>
    /// Data source method. return the list length.
    /// </summary>
    public int GetItemCount()
    {
        return postList.Count;
    }

    /// <summary>
    /// Data source method. Called for a cell every time it is recycled.
    /// Implement this method to do the necessary cell configuration.
    /// </summary>
    public void SetCell(ICell cell, int index)
    {
        //Casting to the implemented Cell
        var item = cell as SF_Cell;
        item.ConfigureCell(postList[index], index);
    }

    #endregion
}
