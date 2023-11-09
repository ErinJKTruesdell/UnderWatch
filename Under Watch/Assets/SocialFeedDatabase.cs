using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SC_LoginSystem;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

public class GetNextImageCommand
{
    public Text usernameText { get; set; }
    public RawImage pictureImage { get; set; }

    public GetNextImageCommand( Text usernameText, RawImage pictureImage)
    {
        this.usernameText = usernameText;
        this.pictureImage = pictureImage;
    }

}

public class SocialFeedDatabase : MonoBehaviour
{
    // Start is called before the first frame update

    string currentPhotoTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    string currentPhotoURL = "";

    string currentPhotoUsername;

    Texture currentPhoto;

    SC_LoginSystem loginSystem;

    bool isWorking = false;


    string rootURL = "https://erinjktruesdell.com/";

    Queue<GetNextImageCommand> queue;

    private void Awake()
    {
        loginSystem = FindObjectOfType<SC_LoginSystem>();
        queue = new Queue<GetNextImageCommand>();
    }

    public void getNextPost(RawImage image, Text text)
    {
        //if (loginSystem != null)
        //{
        Debug.Log("Queueing New Command");
        queue.Enqueue(new GetNextImageCommand(text, image));
       //StartCoroutine(GetRequest(image, text));
        
    }

    public void Update()
    {
        if(!isWorking && queue.Count > 0)
        {
            GetNextImageCommand command = queue.Dequeue();
            Debug.Log("Starting New Command");
            StartCoroutine(GetRequest(command.pictureImage, command.usernameText));
            isWorking = true;
        }
    }

    IEnumerator GetRequest(RawImage image, Text text)
    {
        Debug.Log("Starting Request: " + currentPhotoTimestamp);
        WWWForm form = new WWWForm();
        form.AddField("previousDate", currentPhotoTimestamp);

        //placeholder fake username
        form.AddField("username", "blargj");


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
                    currentPhotoUsername = datachunks[0];
                    currentPhotoURL = datachunks[1];
                    text.text = currentPhotoUsername.Replace("\n", "");
                    Debug.Log("Starting Download");
                    StartCoroutine(downloadImageFromURL(rootURL + currentPhotoURL, image));
                }
                else
                {

                    Debug.Log("Data get failed, releasing queue");
                    isWorking = false;
                }
            }
        }
    }

    IEnumerator downloadImageFromURL(string url, RawImage image)
    {

        Debug.Log("Starting Download Request");
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            currentPhoto = ((DownloadHandlerTexture)request.downloadHandler).texture;
            image.texture = currentPhoto;

        Debug.Log("Releasing Queued Command");
        isWorking = false;

    }
}
