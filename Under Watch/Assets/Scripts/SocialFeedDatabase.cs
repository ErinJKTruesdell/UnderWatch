using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SC_LoginSystem;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using TMPro;

public class GetNextImageCommand
{
    public RawImage profileImage { get; set; }
    public RawImage pictureImage { get; set; }

    public TMP_Text username_text { get; set; }

    public GetNextImageCommand( RawImage profImage, RawImage pictureImage, TMP_Text un)
    {
        this.profileImage = profImage;
        this.pictureImage = pictureImage;
        this.username_text = un;
    }

}

public class SocialFeedDatabase : MonoBehaviour
{
    string currentPhotoTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    string currentPhotoURL = "";

    string currentPhotoProfileURL;

    public string Lat;
    string Long;

    public string currentProfileUsername;

    public string postID = "";

    Texture currentPhoto;

    //SC_LoginSystem loginSystem;

    [SerializeField] PostUIHandling postUIHandling;

    bool isWorking = false;
    public bool isAd = false;

    public string rootURL = "egs01.westphal.drexel.edu/";

    Queue<GetNextImageCommand> queue;

    private void Awake()
    {
       // loginSystem = FindObjectOfType<SC_LoginSystem>();
        queue = new Queue<GetNextImageCommand>();
    }

    private void Start()
    {
        postUIHandling = GameObject.FindObjectOfType<PostUIHandling>();

        if (postUIHandling == null)
        {
            postUIHandling = new PostUIHandling();
        }

        Application.targetFrameRate = 60; // Or Application.targetFrameRate = Screen.currentResolution.refreshRate;

    }

    public string getLocationCoords()
    {
        Debug.Log(Lat);
        return Lat + "°N" + Long + "°W";
    }
    public void getNextPost(RawImage image, RawImage image2, TMP_Text username)
    {
        //if (loginSystem != null)
        //{
        Debug.Log("Queueing New Command");
        queue.Enqueue(new GetNextImageCommand(image2, image, username));
        //StartCoroutine(GetRequest(image, text));

    }

    public void Update()
    {
        if (!isWorking && queue.Count > 0)
        {
            GetNextImageCommand command = queue.Dequeue();
            Debug.Log("Starting New Command");
            StartCoroutine(GetRequest(command.pictureImage, command.profileImage, command.username_text));
            isWorking = true;
        }
    }


    IEnumerator GetRequest(RawImage image, RawImage profImage, TMP_Text usernameText)
    {
        Debug.Log("Starting Request: " + currentPhotoTimestamp);
        WWWForm form = new WWWForm();
        form.AddField("previousDate", currentPhotoTimestamp);
        //placeholder username
        form.AddField("username", "baksdf");

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/get-next-photo.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                //errorMessage = www.error;
                string errorMessage = www.error;
                Debug.Log(errorMessage);
                Debug.Log("FAIL" + www.downloadHandler.text);

                Debug.Log("Data get error, releasing queue");
                isWorking = false;
            }
            else
            {

                //return null
                string responseText = www.downloadHandler.text;

                string[] partition = responseText.Split("@");
                Debug.Log(responseText);
                if (partition.Length > 1)
                {
                    string[] datachunks = partition[0].Split("|");

                    if (datachunks[3] == "Sponsored")
                    {
                        currentPhotoTimestamp = datachunks[2];
                        currentPhotoURL = datachunks[1];
                        isAd = true;
                        StartCoroutine(downloadAdImageFromURL(rootURL + currentPhotoURL, image));
                    }

                    else
                    {
                        currentPhotoProfileURL = datachunks[0];
                        currentPhotoURL = datachunks[1];
                        currentPhotoTimestamp = datachunks[2];
                        currentPhotoProfileURL = currentPhotoProfileURL.Replace("\n", "");
                        currentProfileUsername = datachunks[3];
                        //[4] needs to be split by % for lat/long
                        Lat = datachunks[5].Split('%')[0];
                        Long = datachunks[5].Split('%')[1];
                        postID = datachunks[^1];
                        //Debug.Log("loaded post ID: " + postID);
                        usernameText.text = currentProfileUsername.Trim();

                        string[] reactChunks = partition[1].Split("%");
                        //hey at least its sorta readable

                        postUIHandling.smileLikes = Convert.ToInt32(reactChunks[0].Split(":")[1].Split("|")[0]);
                        postUIHandling.isLikedByLoggedIn.Add(Convert.ToBoolean(reactChunks[0].Split("|")[1]));

                        postUIHandling.thumbLikes = Convert.ToInt32(reactChunks[1].Split(":")[1].Split("|")[0]);
                        postUIHandling.isLikedByLoggedIn.Add(Convert.ToBoolean(reactChunks[1].Split("|")[1]));

                        postUIHandling.fireLikes = Convert.ToInt32(reactChunks[2].Split(":")[1].Split("|")[0]);
                        postUIHandling.isLikedByLoggedIn.Add(Convert.ToBoolean(reactChunks[2].Split("|")[1]));

                        postUIHandling.eyeLikes = Convert.ToInt32(reactChunks[3].Split(":")[1].Split("|")[0]);
                        postUIHandling.isLikedByLoggedIn.Add(Convert.ToBoolean(reactChunks[3].Split("|")[1]));

                        postUIHandling.gatorLikes = Convert.ToInt32(reactChunks[4].Split(":")[1].Split("|")[0]);
                        postUIHandling.isLikedByLoggedIn.Add(Convert.ToBoolean(reactChunks[4].Split("|")[1]));

                        //postUIHandling.OnPostLoad();

                        Debug.Log("Starting Download");
                        StartCoroutine(downloadImageFromURL(rootURL + currentPhotoURL, image, rootURL + currentPhotoProfileURL, profImage));
                    }
                }
                else
                {
                    Debug.Log("Data get failed, releasing queue");

                    isWorking = false;
                }
            }
        }
    }

    IEnumerator downloadImageFromURL(string url1, RawImage image1, string url2, RawImage image2)
    {

        Debug.Log("Starting Download Request");
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url1);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            currentPhoto = ((DownloadHandlerTexture)request.downloadHandler).texture;
        image1.texture = currentPhoto;

        UnityWebRequest request2 = UnityWebRequestTexture.GetTexture(url2);
        yield return request2.SendWebRequest();
        if (request2.isNetworkError || request2.isHttpError)
            Debug.Log(request2.error);
        else
            currentPhoto = ((DownloadHandlerTexture)request2.downloadHandler).texture;
        image2.texture = currentPhoto;
        postUIHandling.doneLoading = true;


        Debug.Log("Releasing Queued Command");
        isWorking = false;

    }

    IEnumerator downloadAdImageFromURL(string url, RawImage image)
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
            currentPhoto = ((DownloadHandlerTexture)request.downloadHandler).texture;

        }
        image.texture = currentPhoto;
    }
}
