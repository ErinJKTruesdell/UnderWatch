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

public class SelfieUploader : MonoBehaviour
{
    public GameManager gm;
    public SC_LoginSystem scls;
    public SelfieCam selfieCam;

    private Coroutine processingCoroutine;

    public searchListItem userListing = new();

    public GameObject blockingPanel;
    public GameObject closeButton;

    public TextMeshProUGUI unText;
    public TextMeshProUGUI targetUNText;
    public RawImage targetPfpImage;
    public TextMeshProUGUI responseText;

    public int facesExpected = 1;

    bool approval;
    void OnEnable()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            gm = new GameManager();
        }
        scls = gm.scls;

        if (scls != null)
        {
            scls.Target += new SC_LoginSystem.TargetHandler(showNewTarget);
        }
        else
        {
            //responseText.color = Color.red;
            //responseText.text = "No target found!";
        }
    }

    public void showNewTarget(string s, EventArgs e)
    {

    }

    public IEnumerator SelfieUpload(string path)
    {
        //checks if null, if not start
        processingCoroutine ??= StartCoroutine(ShowProcessingAnimation());

        responseText.text = "Verifying Image...";

        blockingPanel.SetActive(true);
        unText.text = "@" + SC_LoginSystem.getUsername();

        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;

        if (File.Exists(path))
        {
            Debug.Log("File exists! Uploading Form...");

            WWWForm form = new WWWForm();

            string[] imageNames = path.Split("/");
            string imageName = imageNames[imageNames.Length - 1];
            form.AddBinaryData("file", File.ReadAllBytes(path), imageName);

            form.AddField("username", SC_LoginSystem.getUsername());

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

                //Debug.Log("Form upload complete! " + System.Text.Encoding.ASCII.GetString(www.downloadHandler.data));
                HandleServerResponse(www.downloadHandler.text);
                HandleStartingUIResponse(www.downloadHandler.text);
                closeButton.SetActive(true);
            }
        }
        else
        {
            Debug.Log("File does not exist");
        }
    }
    IEnumerator downloadImageFromURL(string url1, searchListItem li)
    {
        Debug.Log("Starting image Download Request");
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url1);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            li.profilePic.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
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
            //moved this line from the top of TakeSnap(), if anything breaks
            StartCoroutine(scls.doTargetAssignment(SC_LoginSystem.getUsername(), 100));
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



    public void HandleStartingUIResponse(string responseText)
    {
        //this may be a unity bug, but if you try to start a void method that starts a coroutine inside another coroutine, that started coroutine is quietly killed
        //this function is necessary to avoid that bug ~~
        HandleUIServerResponse(responseText);
    }

    public void HandleUIServerResponse(string responseText)
    {
        //APPROVAL: Selfie Not Approved|uploads/67b803a642475733731280.png|uploads/67a6a54aa534c078768223.png|ezkhunter|101
        //Approval. "|" . $username . "|" . $user_prof_url . "|" . $target_id . "|" . $target_prof_url . "|" . $faceCount;

        string[] dataPartition = responseText.Split("|");
        string unData = dataPartition[1].Trim();
        string unPfp = GameManager.rootURL + dataPartition[2].Trim();
        string targetUN = dataPartition[3].Trim();
        string targetPfp = GameManager.rootURL + dataPartition[4].Trim();

        targetUNText.text = targetUN;

        StartCoroutine(downloadImageFromURL(unPfp, userListing));
        //StartCoroutine(downloadImageFromURL(targetPfp, targetPfpImage));
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
        blockingPanel.SetActive(false);
        responseText.text = "";

        selfieCam.InitWebcam();
    }

    [System.Serializable]
    private class ServerResponse
    {
        public bool match;
    }

}
