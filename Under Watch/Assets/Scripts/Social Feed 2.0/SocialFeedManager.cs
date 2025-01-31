using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using PolyAndCode.UI;

public struct ListItemInfo
{
    public RawImage photoImg;
    public RawImage profImage;
    public string unText;
    public string locText;
    public string postIDText;

}

public class SocialFeedManager : MonoBehaviour, IRecyclableScrollRectDataSource
{
    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    [SerializeField]
    private int _dataLength = 15;

    //Dummy data List
    private List<ListItemInfo> _allItems = new List<ListItemInfo>();

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        _recyclableScrollRect.DataSource = this;
    }

    private void Start()
    {
        InitData();
        StartCoroutine(timerChangeLength());
    }

    private void InitData()
    {
        //is this necessary if we are reloading stuff? test.
        if (_allItems != null) _allItems.Clear();

        for (int i = 0; i < _dataLength; i++)
        {
            ListItemInfo obj = new()
            {
                unText = "userName" + i,
                locText = "location" + i,
                postIDText = "item : "
            };
            _allItems.Add(obj);
        }
    }

    //most ideal way of adding items? maybe don't clear the list?? it seems to work regardless...
    //this seems like it could end up storing a LOT of string/image data, and each time it adds more... but will it just be strong enough to not care? 
    IEnumerator timerChangeLength()
    {
        yield return new WaitForSeconds(5);
        _dataLength = 20;
        InitData();
    }

    #region DATA-SOURCE

    /// <summary>
    /// Data source method. return the list length.
    /// </summary>
    public int GetItemCount()
    {
        return _allItems.Count;
    }

    /// <summary>
    /// Data source method. Called for a cell every time it is recycled.
    /// Implement this method to do the necessary cell configuration.
    /// </summary>
    public void SetCell(ICell cell, int index)
    {
        //Casting to the implemented Cell
        var item = cell as SocialPostPopulate;
        item.ConfigureCell(_allItems[index], index);
    }

    #endregion


}
