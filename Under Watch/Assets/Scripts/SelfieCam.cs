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

public class SelfieCam : MonoBehaviour
{
    public RawImage rear;
    WebCamDevice[] devices;

    WebCamTexture webcam;

    public MeshRenderer camMesh;
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public GameManager gm;
    public SC_LoginSystem scls;

    public GameObject testBox;

    public TextMeshProUGUI responseText;
    public GameObject blockingPanel;
    public GameObject closeButton;

    public TextMeshProUGUI unText;
    public TextMeshProUGUI targetUNText;
    public RawImage targetPfpImage;

    public RawImage unPfpImage;


    private bool isOtherSelfie = false;
    public int facesExpected = 1;
    private bool isAngleSelfie = false;
    private bool isLocSelfie = false;

    bool approval = false;

    Vector3 currentLocalEurlerAngles = Vector3.zero;

    void OnEnable()
    {
        InitWebcam();

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

    void InitWebcam()
    {
        devices = WebCamTexture.devices;
        WebCamDevice frontCamera;
        if (devices.Length > 1)
        {
            for (int i = 1; i < devices.Length; i++)
            {
                if (devices[i].isFrontFacing)
                {
                    frontCamera = devices[i];
                    break;
                }
            }
            if (devices[1].name != " ")
            {
                if (UnityEngine.Application.platform == RuntimePlatform.Android)
                    webcam = new WebCamTexture(devices[1].name);
                else
                    webcam = new WebCamTexture(devices[1].name);
                Debug.Log("cam: " + devices[1].name);
            }

            webcam.Play();
            camMesh.material.SetTexture("_MainTex", webcam);
        }
        else
        {
            responseText.color = Color.red;
            responseText.text = "No camera detected";
        }

    }

    public void showNewTarget(string s, EventArgs e)
    {
        if (s.Contains("Success"))
        {
            testBox.SetActive(true);
        }
    }
    public IEnumerator takeSnap()
    {
        yield return frameEnd;
        if (scls != null)
        {
            byte[] bytes = EncodePhoto().EncodeToPNG();
            string loggedInUser = gm.scls.getUsername();

            string filename = loggedInUser + "-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".png";
            string path = Application.persistentDataPath + filename;
            System.IO.File.WriteAllBytes(path, bytes);

            Debug.Log("File Upload Coroutine");
            if (gm.scls != null && gm.scls.getIsLoggedIn())
            {
                responseText.text = "Verifying Image...";
                StartCoroutine(ShowProcessingAnimation());
                blockingPanel.SetActive(true);

                StartCoroutine(SelfieUpload(path));
            }
        }
    }

    IEnumerator SelfieUpload(string path)
    {
        DetermineObjectives();
        StartCoroutine(GetLocation());
        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;

        if (File.Exists(path))
        {
            Debug.Log("File exists! Uploading Form...");

            WWWForm form = new WWWForm();

            string[] imageNames = path.Split("/");
            string imageName = imageNames[imageNames.Length - 1];
            form.AddBinaryData("file", File.ReadAllBytes(path), imageName);

            form.AddField("username", scls.getUsername());

            //always send location
            form.AddField("latitude", latitude.ToString());
            form.AddField("longitude", longitude.ToString());

            form.AddField("angle_selfie", isAngleSelfie.ToString());
            form.AddField("others_selfie", isOtherSelfie.ToString());
            form.AddField("loc_selfie", isLocSelfie.ToString());
            form.AddField("faces_expected", facesExpected);

            UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "uploadImage.php", form);
            Debug.Log("Sending web request...");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                approval = false;
                responseText.text = "Error: " + www.error;

                Debug.Log(www.error);
                StopAllCoroutines(); // Stop the processing animation
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

    void DetermineObjectives()
    {
        isOtherSelfie = false;
        facesExpected = 1;
        isAngleSelfie = false;
        isLocSelfie = false;

        if (DayManager.DoesDayContainObjective(DayManager.ObjTypes.selfieAngle))
        {
            isAngleSelfie = true;
        }
        else if (DayManager.DoesDayContainObjective(DayManager.ObjTypes.selfieOthers))
        {
            isOtherSelfie = true;
            //determine # others expected
            facesExpected = 2;
        }
        else if (DayManager.DoesDayContainObjective(DayManager.ObjTypes.selfieLocation))
        {
            isLocSelfie = true;
        }
    }

    void HandleResponseForObjectives(string response, bool approval)
    {
        string[] dataPartition = response.Split("|");
        string faceCount = dataPartition[5];

        if (isAngleSelfie)
        {
            RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.selfieAngle);
            RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.selfie);
        }
        else if (isOtherSelfie)
        {
            if (approval)
            {
                responseText.text = faceCount + " people detected in selfie!";
                RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.selfieAngle);
            }
            else
            {
                responseText.text = faceCount += " people detected in selfie. Please try again.";
            }
        }
        else if (isLocSelfie)
        {
            if (approval)
            {
                responseText.text = "Correct location detected!";
                RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.selfieLocation);
            }
            else
            {
                responseText.text += " Incorrect location detected. Please try again.";
            }
        }
        else
        {
            if (approval)
            {
                responseText.text = "Your face was detected!";
                RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.selfie);
            }
            else
            {
                responseText.text += "Your face was not detected! Please try again.";
            }
        }
    }

    private Texture2D EncodePhoto()
    {
        Vector3[] corners = new Vector3[4];
        rear.rectTransform.GetWorldCorners(corners);
        Vector3 topLeft = corners[0];

        var width = (int)(corners[3].x - corners[0].x); //.rect.width;
        var height = (int)(corners[1].y - corners[0].y);
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Rescale the size appropriately based on the current Canvas scale
        Vector2 scaledSize = new Vector2(width, height);

        tex.ReadPixels(new Rect(topLeft, scaledSize), 0, 0);
        tex.Apply();
        return tex;
    }

    IEnumerator GetLocation()
    {
        //get last location
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
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
        string unPfp = GameManager.rootURL + dataPartition[3].Trim();
        string targetUN = dataPartition[3].Trim();
        string targetPfp = GameManager.rootURL + dataPartition[4].Trim();

        unText.text = scls.getUsername();
        targetUNText.text = targetUN;

        StartCoroutine(downloadImageFromURL(unPfp, unPfpImage));
        //StartCoroutine(downloadImageFromURL(targetPfp, targetPfpImage));
    }

    public void capturePhoto()
    {
        if (devices.Length > 1)
        {
            Texture2D snap = new Texture2D(webcam.width, webcam.height);
            snap.SetPixels(webcam.GetPixels());
            snap.Apply();
            camMesh.material.SetTexture("_MainTex", snap);
            //byte[] bytes = snap.EncodeToPNG();
            webcam.Stop();

            StartCoroutine(takeSnap());
        }
        else
        {
            responseText.color = Color.red;
            responseText.text = "No camera detected";
        }
    }

    public void CloseBlockerPanel()
    {
        blockingPanel.SetActive(false);
        responseText.text = "";

        InitWebcam();    
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

    void HandleServerResponse(string jsonResponse)
    {
        StopAllCoroutines(); // Stop the processing animation

        //yeah, it's not great, lets fix it later
        if (jsonResponse.Contains("Selfie Approved"))
        {
            responseText.text = "Verification successful";
            approval = true;

            //moved this line from the top of TakeSnap(), if anything breaks
            scls.doTargetAssignment(scls.getUsername(), 100);
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
        HandleResponseForObjectives(jsonResponse, approval);


        /*bool match = false;

        try
        {
            var response = JsonUtility.FromJson<ServerResponse>(jsonResponse);
            match = response.match;
            Debug.Log("114" + response);
        }
        catch
        {
            responseText.text = "Error parsing server response.";
            return;
        }*/

        /*if (match)
        {
            responseText.text = "Verification successful";
        }
        else
        {
            responseText.text = "Verification failed, re-upload or try a different image.";
        }*/
    }

    IEnumerator downloadImageFromURL(string url1, RawImage image1)
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
            image1.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }

    }

    [System.Serializable]
    private class ServerResponse
    {
        public bool match;
    }
}
