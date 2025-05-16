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
    public string adLink;
    //images are assigned in the downloadImages func
    public Texture postPhoto;
    public Texture pfpPhoto;

    public bool isAd = false;
    //reacts
    public Dictionary<string, (int, bool)> ReactNumDict = new()
    {
        {"smile" , (0, false)},
        {"thumb", (0, false) },
        {"gator", (0, false) },
        {"eye", (0, false) },
        {"fire", (0, false) },
    };
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
    public SC_LoginSystem scls;

    //webrequest
    string currentPhotoTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    Queue<SFPostItem> emptyItemsQueue = new();
    bool isFirstLoad = false;
    bool isWorking = false;

    //Recyclable scroll rect's data source must be assigned in Awake.
    private void Awake()
    {
        isFirstLoad = true;
        _recyclableScrollRect.DataSource = this;
        scls = GameObject.FindObjectOfType<SC_LoginSystem>();

    }
    private void Start()
    {
        InitData();

        scrollRect.onValueChanged.AddListener(ListenerMethod);
    }
    public void ListenerMethod(Vector2 value)
    {
        //if scrolled near end
        if (isScrollEnd && !isWorking)
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
        isWorking = true;
        for (int i = 0; i < _dataLength; i++)
        {
            SFPostItem obj = new();
            postList.Add(obj);
            emptyItemsQueue.Enqueue(obj);
        }

        SF_Manager.isScrollEnd = false;

        string localTimestamp = currentPhotoTimestamp;
        while (emptyItemsQueue.Count > 0)
        {
            SFPostItem emptyObj = emptyItemsQueue.Dequeue();
            yield return StartCoroutine(GetRequest(emptyObj, localTimestamp));  // Waits for download to finish
            localTimestamp = currentPhotoTimestamp; // currentPhotoTimestamp is updated in GetRequest
        }
        if (isFirstLoad)
            _recyclableScrollRect.ReloadData();  // Notify scroll list that data is ready
            isFirstLoad = false;
        isWorking = false;
    }


    #region WEB-REQUESTS
    IEnumerator GetRequest(SFPostItem SFitem, string timestamp)
    {
        string usernameStr;
        string latStr;
        string longStr;
        string postIDStr;

        string postImageURL;
        string pfpImageURl;

        Debug.Log("Starting Request: " + timestamp);
        while (scls.getUsername() == null)
        {
            yield return new WaitForEndOfFrame();
        }
        WWWForm form = new WWWForm();
        //form.AddField("previousDate", currentPhotoTimestamp);
        form.AddField("previousDate", timestamp);
        // was originally a placeholder username "asfdasdf"
        form.AddField("username", "asfdasdf");
        form.AddField("loggedInUser", scls.getUsername());

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "/get-next-photo.php", form))
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

                    string newTimeStamp = datachunks[2];
                    currentPhotoTimestamp = newTimeStamp;
                    if (datachunks[0].Contains("Spon"))
                    {
                        //Sponsored|uploads/6802b70799fa1456967581.png|2024-10-18 00:00:00
                        postImageURL = datachunks[1];
                        if (datachunks[3] != "")
                            SFitem.adLink = datachunks[3];
                        if (datachunks[4] != "")
                            SFitem.username = datachunks[4];
                        else
                            SFitem.username = "SnapGram Advertiser";

                        SFitem.isAd = true;
                        yield return StartCoroutine(downloadAdImageFromURL(GameManager.rootURL + postImageURL, SFitem));
                    }

                    else
                    {
                        pfpImageURl = datachunks[0];
                        postImageURL = datachunks[1];
                        pfpImageURl = pfpImageURl.Replace("\n", "");
                        usernameStr = datachunks[3];
                        //[4] needs to be split by % for lat/long
                        latStr = datachunks[5].Split('%')[0];
                        longStr = datachunks[5].Split('%')[1];
                        postIDStr = datachunks[^1];

                        SFitem.username = usernameStr.Trim();
                        SFitem.postID = postIDStr.Trim();
                        SFitem.location = latStr.Trim() + "," + longStr.Trim();

                        string[] reactChunks = partition[1].Split("%");
                        ParseReacts(reactChunks, SFitem);
                        Debug.Log("Starting Download");
                        //possibly make this a yield return to wait until post is fully loaded - faster as is, but less stable?
                        StartCoroutine(downloadPostimage(GameManager.rootURL + postImageURL, SFitem));
                        StartCoroutine(downloadPfpImage(GameManager.rootURL + pfpImageURl, SFitem));
                    }
                }
                else
                {
                    Debug.Log("Data get failed, releasing queue");
                }
            }
        }
    }

    void ParseReacts(string[] reactChunks, SFPostItem postItem)
    {
        int i = 0;
        List<string> listOfKeys = new(postItem.ReactNumDict.Keys);
        string debugList = "";

        foreach (string key in listOfKeys)
        {
            int likes = Convert.ToInt32(reactChunks[i].Split(":")[1].Split("|")[0]);
            bool isUserLiked = Convert.ToBoolean(reactChunks[i].Split("|")[1]);

            postItem.ReactNumDict[key] = (likes, isUserLiked);
            i++;

            debugList += postItem.ReactNumDict[key];
        }

       // Debug.Log("reacts: " + debugList);
    }

    IEnumerator downloadPostimage(string url1, SFPostItem item)
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
    }
    IEnumerator downloadPfpImage(string url2, SFPostItem item)
    {
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
