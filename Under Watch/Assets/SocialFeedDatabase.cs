using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SC_LoginSystem;
using UnityEngine.Networking;

public class SocialFeedObject
{
    public string timestamp;
    public string username;
    public Texture photo;

    public SocialFeedObject()
    {
        this.timestamp = "";
        this.username = string.Empty;
        this.photo = null;
    }

    public SocialFeedObject(string timestamp, string username, Texture photo)
    {
        this.timestamp = timestamp;
        this.username = username;
        this.photo = photo;
    }
}

public class SocialFeedDatabase : MonoBehaviour
{
    // Start is called before the first frame update

    string currentPhotoTimestamp = "";

    string currentPhotoURL = "";

    string currentPhotoUsername;

    Texture currentPhoto;

    bool isWorking = false;


    string rootURL = "https://erinjktruesdell.com/";

    public SocialFeedObject getNextPost()
    {
        StartCoroutine(GetRequest());
        isWorking = true;

        while (isWorking)
        {
            // wait. turn an async function into a sync one. there's probably a better way to do this. 
        }

        return new SocialFeedObject(currentPhotoTimestamp,currentPhotoUsername,currentPhoto);
    }

    IEnumerator GetRequest()
    {
        WWWForm form = new WWWForm();
        form.AddField("datetime", currentPhotoTimestamp);

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "getPhoto.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                //errorMessage = www.error;
                isWorking = false;
            }
            else
            {
                //return null
                isWorking = false;
            }
        }
    }

    IEnumerator downloadImageFromURL(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            currentPhoto = ((DownloadHandlerTexture)request.downloadHandler).texture;

    }
}
