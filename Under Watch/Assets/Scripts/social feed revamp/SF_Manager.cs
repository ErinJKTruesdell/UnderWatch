using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;
using System;
using UnityEngine.Networking;
public class SFPostItem
{
    //use TextMeshProUGUI instead of TMP_Text because it is a concrete component, not abstract base class
    //strings are assigned in SF_cell
    public string username;
    public string location;
    public string postID;

    //images are assigned in the downloadImages func
    public Texture postPhoto;
    public Texture pfpPhoto;


}

public class SF_Manager : MonoBehaviour, IRecyclableScrollRectDataSource
{
    //recycle data list stuff
    [SerializeField]
    RecyclableScrollRect _recyclableScrollRect;

    [SerializeField]
    private int _dataLength;

    private List<SFPostItem> postList = new();

    public static bool isScrollEnd = false;

    public RecyclableScrollRect scrollRect;

    //webrequest
    public string rootURL = "egs01.westphal.drexel.edu/";
    string currentPhotoTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    Queue<SFPostItem> emptyItemsQueue = new();
    bool isFirstLoad = false;

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        InitData();
        isFirstLoad = true;
        _recyclableScrollRect.DataSource = this;
    }
    private void Update()
    {
        if (isScrollEnd)
        {
            Debug.Log("scrolling!");
        }
    }
    private void Start()
    {
        scrollRect.onValueChanged.AddListener(ListenerMethod);
    }
    public void ListenerMethod(Vector2 value)
    {
        //if scrolled near end
        if (isScrollEnd)
        {
            Debug.Log("scrolling at end!");
            InitData();
            SF_Manager.isScrollEnd = false;
        }
    }
    //Initialising postList with dummy data 
    public void InitData()
    {
        StartCoroutine(GetRequestAndAdd());
    }
    IEnumerator GetRequestAndAdd()
    {
        for (int i = 0; i < _dataLength; i++)
        {
            SFPostItem obj = new();
            postList.Add(obj);
            emptyItemsQueue.Enqueue(obj);
        }

        SF_Manager.isScrollEnd = false;
        string debugStr = "";
        foreach (SFPostItem itm in emptyItemsQueue)
        {
            debugStr += itm.username + ", ";
        }
        Debug.Log(debugStr);
        while (emptyItemsQueue.Count > 0)
        {
            SFPostItem emptyObj = emptyItemsQueue.Dequeue();
            yield return StartCoroutine(GetRequest(emptyObj));  // Waits for download to finish
            Debug.Log("added post: " + emptyObj.postPhoto + " added pfp: " + emptyObj.pfpPhoto);
        }
        if (isFirstLoad)
            _recyclableScrollRect.ReloadData();  // Notify scroll list that data is ready
            isFirstLoad = false;
    }


    #region WEB-REQUESTS
    IEnumerator GetRequest(SFPostItem SFitem)
    {
        bool isAd = false;

        string usernameStr;
        string latStr;
        string longStr;
        string postIDStr;

        string postImageURL;
        string pfpImageURl;

        Debug.Log("Starting Request: " + currentPhotoTimestamp);

        WWWForm form = new WWWForm();
        //form.AddField("previousDate", currentPhotoTimestamp);
        form.AddField("previousDate", currentPhotoTimestamp);
        //placeholder username
        form.AddField("username", "baksdf");

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/get-next-photo.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                string errorMessage = www.error;
                Debug.Log(errorMessage);
                Debug.Log("FAIL" + www.downloadHandler.text);

                Debug.Log("Data get error");
            }
            else
            {
                //return null
                string responseText = www.downloadHandler.text;

                string[] partition = responseText.Split("@");
                Debug.Log("Response: " + responseText);
                if (partition.Length > 1)
                {
                    string[] datachunks = partition[0].Split("|");

                    if (datachunks[3] == "Sponsored")
                    {
                        currentPhotoTimestamp = datachunks[2];
                        postImageURL = datachunks[1];
                        isAd = true;
                        yield return StartCoroutine(downloadAdImageFromURL(rootURL + postImageURL, SFitem));
                    }

                    else
                    {
                        pfpImageURl = datachunks[0];
                        postImageURL = datachunks[1];
                        currentPhotoTimestamp = datachunks[2];
                        pfpImageURl = pfpImageURl.Replace("\n", "");
                        usernameStr = datachunks[3];
                        //[4] needs to be split by % for lat/long
                        latStr = datachunks[5].Split('%')[0];
                        longStr = datachunks[5].Split('%')[1];
                        postIDStr = datachunks[^1];
                        //previousPostId = Convert.ToInt32(datachunks[^1]);
                        /*Debug.Log("pfp:" + pfpImageURl);
                        Debug.Log("post: " + postImageURL);
                        Debug.Log("time: " + currentPhotoTimestamp);
                        Debug.Log("un: " + usernameStr);
                        Debug.Log("loc: " + latStr + longStr);
                        Debug.Log("id: " + postIDStr);*/

                        SFitem.username = usernameStr.Trim();
                        SFitem.postID = postIDStr.Trim();
                        SFitem.location = latStr.Trim() + "," + longStr.Trim();

                        string[] reactChunks = partition[1].Split("%");

                        Debug.Log("Starting Download");
                        //possibly don't make this wait for the download image to complete? as is seems fastest, but it shouldn't be!
                        yield return StartCoroutine(downloadImageFromURL(rootURL + postImageURL, rootURL + pfpImageURl, SFitem));
                    }
                }
                else
                {
                    Debug.Log("Data get failed, releasing queue");
                }
            }
        }
    }

    IEnumerator downloadImageFromURL(string url1, string url2, SFPostItem item)
    {
        Debug.Log("Starting Download Request");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url1);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log("error: " + url1 + request.error);
        }
        else
        {
            Texture image1Download = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (image1Download == null)
            {
                Debug.Log("Post photo texture is null after download");
            }

            item.postPhoto = image1Download;
        }

        UnityWebRequest request2 = UnityWebRequestTexture.GetTexture(url2);
        yield return request2.SendWebRequest();
        if (request2.isNetworkError || request2.isHttpError)
        {
            Debug.Log("error: " + url2 + request2.error);
        }
        else
        {
            Texture image2Download = ((DownloadHandlerTexture)request2.downloadHandler).texture;
            if (image2Download == null)
            {
                Debug.Log("Pfp photo texture is null after download");
            }

            item.pfpPhoto = image2Download;
        }

        Debug.Log("Download complete");
    }

    IEnumerator downloadAdImageFromURL(string url, SFPostItem item)
    {
        Debug.Log("Starting Ad Download Request");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Texture image1Download = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (image1Download == null)
            {
                Debug.Log("Post photo texture is null after download");
            }

            item.postPhoto = image1Download;
        }
    }
    #endregion

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
