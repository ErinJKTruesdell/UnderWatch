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
    public UserInfo posterUser;
    public UserInfo targetUser;
    public string placeName;
    public float latitude;
    public float longitude;
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

    public RecyclableScrollRect scrollRect;
    public SC_LoginSystem scls;

    //webrequest
    int lastSeenPostID = 999999999; // Start with int.MaxValue to fetch newest
    Queue<SFPostItem> emptyItemsQueue = new();
    Queue<(string url, Action<Texture> onComplete)> imageQueue = new();
    bool isDownloadingImage = false;
    bool isFirstLoad = false;
    bool isWorking = false;
    [SerializeField] private float loadThreshold = 0.1f; // 10% from bottom


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
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    private void OnScroll(Vector2 scrollPos)
    {
        // Only vertical matters here
        float verticalPos = scrollRect.verticalNormalizedPosition;

        if (verticalPos <= loadThreshold && !isWorking)
        {
            Debug.Log("Scrolled near bottom. Loading more content...");
            isWorking = true;
            InitData(); // Load next batch
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

        while (emptyItemsQueue.Count > 0)
        {
            SFPostItem emptyObj = emptyItemsQueue.Dequeue();
            yield return StartCoroutine(GetRequest(emptyObj));  // Waits for download to finish
        }
        if (isFirstLoad)
            _recyclableScrollRect.ReloadData();  // Notify scroll list that data is ready
            isFirstLoad = false;
        isWorking = false;
    }


    #region WEB-REQUESTS
    IEnumerator GetRequest(SFPostItem SFitem)
    {
        string postImageURL;
        string pfpImageURl;

        Debug.Log("Starting Request: " + lastSeenPostID);
        while (GameManager.loggedInUser == null)
        {
            yield return new WaitForEndOfFrame();
        }
        WWWForm form = new WWWForm();
        form.AddField("last_seen_post_id", lastSeenPostID);
        form.AddField("username", "asfdasdf");
        form.AddField("loggedInUser", GameManager.loggedInUser.un);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-next-photo.php", form))
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

                    string receivedDate = datachunks[2];
                    if (datachunks[0].Contains("Sponsored"))
                    {
                        //Sponsored|uploads/6802b70799fa1456967581.png|2024-10-18 00:00:00
                        postImageURL = datachunks[1].Trim();
                        if (datachunks[3] != "")
                            SFitem.adLink = datachunks[3];
                        if (datachunks[4] != "")
                            SFitem.posterUser = new UserInfo(
                                datachunks[4],
                                "Snap",
                                "Gram"
                            );
                        else
                            SFitem.posterUser = new UserInfo(
                                "SnapGram Advertiser",
                                "Snap",
                                "Gram"
                            );

                        SFitem.isAd = true;
                        EnqueueImageDownload(GameManager.rootURL + postImageURL, tex => SFitem.postPhoto = tex);
                    }

                    else
                    {
                        try
                        {
                           // echo $user_pfp. "|".$post_image_url. "|".$date_tmp. "|". $poster_un. "|" $poster_fn. "|". $poster_ln. "|".  $target_un. "|". $target_fn. "|". $target_ln. "|". $target_prof."|. $lat_tmp." % ".$long_tmp." % ".$place_name ." | ". $post_id."@".$likes_data;
                            pfpImageURl = datachunks[0].Trim();
                            pfpImageURl = pfpImageURl.Replace("\n", "").Trim();

                            postImageURL = datachunks[1].Trim();

                            string usernameStr = datachunks[3];
                            string uFN = datachunks[4];
                            string uLN = datachunks[5];

                            string targetUNTStr = datachunks[6];
                            string tFN = datachunks[7];
                            string tLN = datachunks[8];
                            string targetPFP = datachunks[9];

                            //[4] needs to be split by % for lat/long
                            string latStr = datachunks[10].Split('%')[0];
                            string longStr = datachunks[10].Split('%')[1];
                            string placeName = datachunks[10].Split('%')[2];

                            string postIDStr = datachunks[^1];
                            lastSeenPostID = Convert.ToInt32(postIDStr);

                            SFitem.posterUser = new UserInfo(
                                usernameStr.Trim(),
                                uFN.Trim(),
                                uLN.Trim()
                            );

                            if (!string.IsNullOrWhiteSpace(targetUNTStr))
                            {
                                SFitem.targetUser = new UserInfo(
                                    targetUNTStr.Trim(),
                                    tFN.Trim(),
                                    tLN.Trim()
                                );
                            }

                            if (!string.IsNullOrWhiteSpace(placeName))
                            {
                                SFitem.latitude = float.Parse(latStr.Trim());
                                SFitem.longitude = float.Parse(longStr.Trim());
                                SFitem.placeName = placeName.Trim();
                            }

                            Debug.Log($"Poster: {SFitem.posterUser?.un}");
                            SFitem.postID = postIDStr.Trim();

                            string[] reactChunks = partition[1].Split("%");
                            ParseReacts(reactChunks, SFitem);
                            Debug.Log("Starting Download");
                            //possibly make this a yield return to wait until post is fully loaded - faster as is, but less stable?
                            EnqueueImageDownload(GameManager.rootURL + postImageURL, tex => SFitem.postPhoto = tex);
                            EnqueueImageDownload(GameManager.rootURL + pfpImageURl, tex => SFitem.pfpPhoto = tex);
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Error parsing data: " + e.Message);
                            ErrorEventHandler.InvokeError("Server Return Error!", "Unexpected data format. Please try again, or check your internet connection.", Color.red);
                            yield break; // Exit if parsing fails
                        }
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

    void EnqueueImageDownload(string url, Action<Texture> callback)
    {
        imageQueue.Enqueue((url, callback));
        if (!isDownloadingImage)
            StartCoroutine(ProcessImageQueue());
    }

    IEnumerator ProcessImageQueue()
    {
        isDownloadingImage = true;

        while (imageQueue.Count > 0)
        {
            var (url, onComplete) = imageQueue.Dequeue();
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    onComplete?.Invoke(tex);
                }
            }

            yield return new WaitForSeconds(0.2f); // slight delay to reduce spike
        }

        isDownloadingImage = false;
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
