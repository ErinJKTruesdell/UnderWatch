using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OnlineMapsGPXObject;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static SC_LoginSystem;
using TMPro;
using UnityEngine.UI;

public class GetTargetLocation : MonoBehaviour
{
    public GameObject mapObj;
    public GameObject mapOverlay;
    public OnlineMaps map;

    public float targetLat;
    public float targetLong;

    public GameObject showMapButton;

    SC_LoginSystem sclogin;

    public TMP_Text targetText;
    public RawImage targetProf;

    public GameObject infiniteScrollViewport;

    // Start is called before the first frame update
    void Start()
    {
        mapObj.SetActive(false);
        mapOverlay.SetActive(false);
        showMapButton.SetActive(true);
        infiniteScrollViewport.SetActive(true);
        sclogin = GameObject.FindObjectOfType<SC_LoginSystem>();
        if (sclogin == null)
        {
            sclogin = new SC_LoginSystem();
        }
    }


    public void hideMap()
    {
        mapObj.SetActive(false);
        mapOverlay.SetActive(false);
        showMapButton.SetActive(true);
        infiniteScrollViewport.SetActive(true);
    }

    public void showMap()
    {
        //get target location
        StartCoroutine(LocateTarget(SC_LoginSystem.getUsername()));
        Debug.Log(SC_LoginSystem.getUsername());

        //turn lolcation on
        infiniteScrollViewport.SetActive(false);
        mapOverlay.SetActive(true);
        mapObj.SetActive(true);
        showMapButton.SetActive(false);

        //set lat and long
        map.SetPosition(39.952f, -75.15f); //39.952f, -75.15f
        map.markerManager.Add(new OnlineMapsMarker());
        map.markerManager[0].SetPosition(39.952f, -75.15f);
        map.markerManager[0].scale = 0.12f;

    }

    public void reCenter()
    {

        map.SetPosition(targetLong, targetLat);
    }

    public IEnumerator LocateTarget(string username)
    {


        WWWForm form = new WWWForm();
        form.AddField("username", username);

        string errorMessage = "";

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-target-location.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
            }
            //else
            // {
            string responseText = www.downloadHandler.text;
            Debug.Log("response: " + responseText);
            if (responseText.Contains("Success"))
            {
                string[] dataChunks = responseText.Split('|');
                targetText.text = "@" + dataChunks[1];
                string targetTimestamp = dataChunks[4];

                //turn lolcation on
                infiniteScrollViewport.SetActive(false);
                mapOverlay.SetActive(true);
                mapObj.SetActive(true);
                showMapButton.SetActive(false);

                if (dataChunks[2] != "" && dataChunks[3] != "")
                {
                    targetLat = float.Parse(dataChunks[2]);
                    targetLong = float.Parse(dataChunks[3]);

                    //set lat and long
                    map.SetPosition(targetLong, targetLat); //2.35, 48.87
                    map.markerManager.Add(new OnlineMapsMarker());
                    map.markerManager[0].SetPosition(targetLong, targetLat);
                    map.markerManager[0].scale = 0.12f;
                }
                else
                {
                    map.SetPosition(0, 0); //2.35, 48.87
                    map.markerManager.Add(new OnlineMapsMarker());
                    map.markerManager[0].SetPosition(0, 0);
                    map.markerManager[0].scale = 0.12f;
                }
                //get target's profile pic
                string profUrl;
                if (dataChunks[8] != "")
                {
                    profUrl = GameManager.rootURL + dataChunks[8];
                    Debug.Log("prof: " + GameManager.rootURL + profUrl);
                    StartCoroutine(downloadImageFromURL(profUrl, targetProf));
                }
            }
            else
            {
                errorMessage = responseText;
                Debug.Log("Error Line 110: " + errorMessage);
            }
            //}
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

    public void ClickOnProfile()
    {
        if (string.IsNullOrEmpty(targetText.text))
        {
            ShowClickedProfile.userName = "";
            Debug.Log("Target username not found");
        }
        else
        {
            ShowClickedProfile.userName = targetText.text;

        }
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }
}
