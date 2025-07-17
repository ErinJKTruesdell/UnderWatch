using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Net;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening;
using System.Text;
using UnityEngine.Android;
using System.Linq;

public class SelfieUploader : MonoBehaviour
{
    public GameManager gm;
    public SC_LoginSystem scls;
    public SelfieCam selfieCam;

    private Coroutine processingCoroutine;

    public searchListItem userListing = new();

    public GameObject blockingPanel;
    public GameObject closeButton;

    public GameObject profileObjects;
    float profileObjsStartingPos;
    public TextMeshProUGUI responseText;
    public TextMeshProUGUI unText;
    public TextMeshProUGUI targetUNText;
    public TextMeshProUGUI targetNameText;

    public RawImage targetPfpImage;
    public RawImage userPfpImage;

    string locationName = "";
    string imagePath = "";
    float latitude;
    float longitude;

    public int facesExpected = 2;

    bool approval;
    void OnEnable()
    {
        if (gm == null)
        {
            gm = FindObjectOfType<GameManager>();
        }
        if (scls == null)
        {
            scls = FindObjectOfType<SC_LoginSystem>();
        }
        if (selfieCam == null)
        {
            selfieCam = FindObjectOfType<SelfieCam>();
        }

        profileObjects.transform.localPosition = new Vector3(0, Screen.height * 3, 0);
    }

    public IEnumerator SelfieGetLocation(string path)
    {
        //UI elements
        processingCoroutine ??= StartCoroutine(ShowProcessingAnimation());
        responseText.color = Color.white;
        responseText.text = "Verifying Image...";
        blockingPanel.SetActive(true);
        Debug.Log("hello:");
        profileObjects.SetActive(true);
        profileObjects.transform.DOLocalMoveY(450, 1f).SetEase(Ease.OutQuad);

        unText.text = "@" + GameManager.loggedInUser.un;
        userPfpImage.texture = GameManager.loggedInUser.profilePic;

        targetUNText.text = GameManager.currTarget.un;
        targetPfpImage.texture = GameManager.currTarget.profilePic;

        imagePath = path;

#if !UNITY_EDITOR
        yield return new WaitUntil(() => Input.location.status != LocationServiceStatus.Initializing);

        // get location info for posting
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
#elif UNITY_EDITOR
        // For testing in the editor, use a fixed location
        latitude = 39.7749f; // Example: San Francisco latitude
        longitude = -79.4194f; // Example: San Francisco longitude
        ConvertCoordinates.StartGeocodeRequest(latitude, longitude, SelfieUploadAfterLocation);
#endif
        locationName = "";

        ConvertCoordinates.StartGeocodeRequest(latitude, longitude, SelfieUploadAfterLocation);

        yield return new WaitForSeconds(.01f); 
    }

    void SelfieUploadAfterLocation(List<string> locationNames)
    {
        if (locationNames.Count == 1)
        {
            //this means it is a premise
            locationName = locationNames[0];
            Debug.Log("Location found: " + locationName);
        }
        else if (locationNames.Count > 1)
        {
            //this means it is a street address
            locationName = string.Join(", ", locationNames);
            Debug.Log("Location found: " + locationName);
        }
        else
        {
            ErrorEventHandler.InvokeError("Geocoding Error!", "No location found for coordinates", Color.red);
        }
        StartCoroutine(SelfieUpload());
    }

    public IEnumerator SelfieUpload()
    {
        if (File.Exists(imagePath))
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }

            Debug.Log("File exists! Uploading Form..." + GameManager.loggedInUser.un);

            WWWForm form = new WWWForm();

            string[] imageNames = imagePath.Split("/");
            string imageName = imageNames[imageNames.Length - 1];

            form.AddBinaryData("file", File.ReadAllBytes(imagePath), imageName);
            form.AddField("username", GameManager.loggedInUser.un);

            form.AddField("latitude", latitude.ToString());
            form.AddField("longitude", longitude.ToString());
            form.AddField("place_name", locationName.ToString());

            form.AddField("faces_expected", facesExpected);

            UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "uploadImage.php", form);
            Debug.Log("Sending web request...");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                approval = false;
                ErrorEventHandler.InvokeError("Server Error:", www.error, Color.red);
                Debug.Log(www.error + " " + www.downloadHandler.text);

                if (processingCoroutine != null)
                {
                    StopCoroutine(processingCoroutine);
                    processingCoroutine = null;
                }
                closeButton.SetActive(true);
            }
            else
            {
                Debug.Log("result: " + www.result);
                Debug.Log("response: " + www.downloadHandler.text);

                HandleServerResponse(www.downloadHandler.text);

                closeButton.SetActive(true);
            }
        }
        else
        {
            Debug.Log("File does not exist");
        }
    }
    void HandleServerResponse(string jsonResponse)
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
            processingCoroutine = null;
        }

        //$isUserInPic ."|". $isTargetInPic ."|". $faceCount
        if (jsonResponse.Contains("Selfie Approved"))
        {
            responseText.text = "Verification successful";
            approval = true;
        }
        else if (jsonResponse.Contains("Selfie Not Approved"))
        {
            responseText.text = "Verification failed, re-upload or try a different image.";
            approval = false;
        }
        else
        {
            responseText.text = "Error parsing server response.";
            approval = false;
        }
        HandleStartingUIResponse(jsonResponse);
        HandleFaceCount(jsonResponse);
    }
    public void HandleStartingUIResponse(string serverResponse)
    {
        //this may be a unity bug, but if you try to start a void method that starts a coroutine inside another coroutine, that started coroutine is quietly killed
        //this function is necessary to avoid that bug ~~
        HandleUIServerResponse(serverResponse);
    }

    public void HandleUIServerResponse(string serverResponse)
    {
        //Approval.  "|" $isUserInPic ."|". $isTargetInPic;
        HandleTargetUIResponse();
    }

    void HandleTargetUIResponse()
    {
        if (approval)
        {
            StartCoroutine(scls.doTargetAssignment(GameManager.loggedInUser.un));
            PointsManager.AddPoints(GameManager.loggedInUser.un, PointsManager.capturePoints, PointsManager.Source.TargetCapture);
            PointsManager.AddPoints(targetUNText.text, PointsManager.beCapturedPoints, PointsManager.Source.BeCaptured);
        }
    }

    void HandleFaceCount(string jsonResponse)
    {
        string[] parts = jsonResponse.Split('|');
        if (parts.Length >= 3)
        {
            int faceCount;
            if (int.TryParse(parts[2], out faceCount))
            {
                Debug.Log("Face count: " + faceCount);

                var targetAch = AchievementManager.allAchievements
                .FirstOrDefault(a => a.pointSource == PointsManager.Source.AchSocialButterfly);

                for (int i = targetAch.reqsPerLevel.Count - 1; i >= 0; i--)
                {
                    //iterates backwards thru the list to catch the most impressive selfie first
                    if (faceCount >= targetAch.reqsPerLevel[i]) 
                    {
                        AchievementEventHandler.InvokeAddToAchievment(PointsManager.Source.AchGramMaster, faceCount, resetCount: true);
                        StartCoroutine(SendSelfieCountToServer(faceCount));
                        break;
                    }
                }
            }
            else
            {
                ErrorEventHandler.InvokeError("Face Count Error", "Unexpected number of faces in photo", Color.white);
            }
        }
        else
        {
            Debug.Log("Unexpected server response format: " + jsonResponse);
            ErrorEventHandler.InvokeError("Server Error", "Unexpected response format: " + jsonResponse, Color.red);
        }
    }

    IEnumerator SendSelfieCountToServer(int faces)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);
        form.AddField("faces", faces);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "post-selfie-count.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to send selfies: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                Debug.Log("Selfies sent successfully: " + response + " min");
            }
        }
    }


    IEnumerator ShowProcessingAnimation()
    {
        int dotCount = 0;
        while (true)
        {
            responseText.text = "Verifying Image" + new string('.', dotCount % 4);
            dotCount++;
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void CloseBlockerPanel()
    {
        closeButton.SetActive(false);
        blockingPanel.SetActive(false);
        profileObjects.transform.DOLocalMoveY(Screen.height * 3, .5f).SetEase(Ease.OutQuad)
            .OnComplete(() => profileObjects.SetActive(false));

        responseText.text = "";

        selfieCam.InitWebcam();
    }

    [System.Serializable]
    private class ServerResponse
    {
        public bool match;
    }
}