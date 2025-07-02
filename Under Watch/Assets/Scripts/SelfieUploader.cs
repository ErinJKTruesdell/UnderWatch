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

    public TextMeshProUGUI responseText;

    public TextMeshProUGUI unText;
    public TextMeshProUGUI targetUNText;
    public TextMeshProUGUI targetNameText;

    public RawImage targetPfpImage;
    public RawImage userPfpImage;

    public int facesExpected = 1;

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
    }

    public IEnumerator SelfieUpload(string path)
    {
        //checks if null, if not start
        processingCoroutine ??= StartCoroutine(ShowProcessingAnimation());

        responseText.color = Color.white;
        responseText.text = "Verifying Image...";

        blockingPanel.SetActive(true);
        profileObjects.SetActive(true);
        profileObjects.transform.DOLocalMoveY(Screen.height * 3, 1.3f).SetEase(Ease.OutQuad).From();

        unText.text = "@" + GameManager.loggedInUser.un;
        
        //will getting the new user cause issues?
        targetUNText.text = GameManager.currTarget.un;
        targetPfpImage.texture = GameManager.currTarget.profilePic;

        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;

        if (File.Exists(path))
        {
            Debug.Log("File exists! Uploading Form...");

            WWWForm form = new WWWForm();

            string[] imageNames = path.Split("/");
            string imageName = imageNames[imageNames.Length - 1];
            form.AddBinaryData("file", File.ReadAllBytes(path), imageName);
            form.AddField("username", GameManager.loggedInUser.un);
            //always send location
            form.AddField("latitude", latitude.ToString());
            form.AddField("longitude", longitude.ToString());

            form.AddField("faces_expected", facesExpected);

            UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "uploadImage.php", form);
            Debug.Log("Sending web request...");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                approval = false;
                responseText.text = "Error: " + www.error;
                Debug.Log(www.error);

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

        //yeah, it's not great, lets fix it later
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
            StartCoroutine(scls.doTargetAssignment(GameManager.loggedInUser.un, 100));
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
        profileObjects.transform.DOLocalMoveY(Screen.height * 3, 1.3f).SetEase(Ease.OutQuad)
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