using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProfileDatabase : MonoBehaviour
{

    public RawImage profileImage;
    public RawImage zoomedImage;
    public GameObject zoomedImageObject;

    public GameObject photoPrefab; // has RawImage component that holds actual image
    public Transform contentTransform;

    string rootURL = "https://erinjktruesdell.com/";

    public TMP_Text usernameText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void fillCanvas(string username)
    {
        Debug.Log("Filling Canvas: " + username);
        StartCoroutine(getAndDownloadImages(username));
    }

    


    private IEnumerator getAndDownloadImages(string username)
    {
        // get data from server
        WWWForm form = new WWWForm();
        form.AddField("username", username); //dummy data

        usernameText.text = "@" + username;
        usernameText.gameObject.SetActive(true);

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/get-all-user-photos.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                //errorMessage = www.error;
                string errorMessage = www.error;
                Debug.Log(errorMessage);

                Debug.Log("Data get error, releasing queue");
            }
            else
            {
                //get response
                string responseText = www.downloadHandler.text;
                Debug.Log(responseText);
                string[] userChunks = responseText.Split('|');

                //resize content
                contentTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(216, Mathf.Floor(userChunks.Length/3f) * 72);

                string profUrl = "/" + userChunks[0].Substring(1);
                StartCoroutine(downloadImageFromURL(rootURL + profUrl,profileImage));

                //create prefab and load images
                for (int s = 1; s<userChunks.Length; s++)
                {
                    string i = userChunks[s];
                    if (i != "")
                    {
                        GameObject picItem = Instantiate(photoPrefab) as GameObject;
                        picItem.transform.SetParent(contentTransform, false);

                        PlayerPhotoProfile ppp = picItem.GetComponent<PlayerPhotoProfile>();
                        ppp.zoomedImage = zoomedImage;
                        ppp.zoomedImageObj = zoomedImageObject;
                        //downlaod prof img
                        StartCoroutine(downloadImageFromURL(rootURL + i, ppp.thisImage));
                    }
                    }

                }
            
        }

    }

    private IEnumerator downloadImageFromURL(string url1, RawImage image1)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url1);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            image1.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }

    }
}
