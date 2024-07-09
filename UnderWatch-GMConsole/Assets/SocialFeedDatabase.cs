using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    // Start is called before the first frame update

    string currentPhotoTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    string currentPhotoURL = "";

    string currentPhotoProfileURL;

    string currentProfileUsername;
    public PlayerViewTab pvt;

    //string 

    Texture currentPhoto;


    bool isWorking = false;


    string rootURL = "https://erinjktruesdell.com/";

    Queue<GetNextImageCommand> queue;

    private void Awake()
    {
        queue = new Queue<GetNextImageCommand>();
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
        if(!isWorking && queue.Count > 0)
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

        //placeholder fake username
        form.AddField("username", pvt.loggedinUser);


        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/get-next-photo.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                //errorMessage = www.error;
                string errorMessage = www.error;
                Debug.Log(errorMessage);

                Debug.Log("Data get error, releasing queue");
                isWorking = false;
            }
            else
            {
                //return null
                string responseText = www.downloadHandler.text;
                string[] datachunks = responseText.Split("|");
                //Debug.Log(datachunks.Length);
                if (datachunks.Length > 1)
                {
                    currentPhotoTimestamp = datachunks[2];
                    currentPhotoProfileURL = datachunks[0];
                    currentPhotoURL = datachunks[1];
                    currentPhotoProfileURL = currentPhotoProfileURL.Replace("\n", "");
                    currentProfileUsername = datachunks[3];
                    usernameText.text = currentProfileUsername;

                    Debug.Log("Starting Download");
                    StartCoroutine(downloadImageFromURL(rootURL + currentPhotoURL, image, rootURL + currentPhotoProfileURL, profImage));
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

        Debug.Log("Releasing Queued Command");
        isWorking = false;

    }
}
