using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Net;

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
    public RawImage unPfpImage;
    public RawImage targetPfpImage;

    Vector3 currentLocalEurlerAngles = Vector3.zero;
    // Start is called before the first frame update
    void Start()
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
            responseText.color = Color.red;
            responseText.text = "No target found!";
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

            byte[] bytes = tex.EncodeToPNG();
            string filename = gm.scls.getUsername() + "-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".png";
            string path = Application.persistentDataPath + filename;
            Debug.Log("--------------------------------------SAVING TO PATH--------------------------------------");
            Debug.Log(path);
            Debug.Log("------------------------------------------------------------------------------------------");
            System.IO.File.WriteAllBytes(path, bytes);


            //get last location
            Input.location.Start();

            // Waits until the location service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;

            //upload to server
            Debug.Log("Getting logged in user...");
            string loggedInUser = gm.scls.getUsername();

            Debug.Log("File Upload Coroutine");
            if (gm.scls != null && gm.scls.getIsLoggedIn())
            {
                Debug.Log(path);

                responseText.text = "Verifying Image...";
                StartCoroutine(ShowProcessingAnimation());
                blockingPanel.SetActive(true);

                if (File.Exists(path))
                {
                    Debug.Log("File exists! Uploading Form...");
                    WWWForm form = new WWWForm();
                    string[] imageNames = path.Split("/");
                    string imageName = imageNames[imageNames.Length - 1];
                    form.AddBinaryData("file", File.ReadAllBytes(path), imageName);
                    form.AddField("username", scls.getUsername());
                    form.AddField("latitude", latitude.ToString());
                    form.AddField("longitude", longitude.ToString());

                    UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "uploadImage.php", form);
                    Debug.Log("Sending web request...");

                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        responseText.text = "Error: " + www.error;

                        Debug.Log(www.error);
                        StopAllCoroutines(); // Stop the processing animation
                        closeButton.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("result: " + www.result);

                        Debug.Log("response: " + www.downloadHandler.text);

                        Debug.Log("Form upload complete! " + System.Text.Encoding.ASCII.GetString(www.downloadHandler.data));
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
        //Approval. "|" . $username . "|" . $user_prof_url . "|" . $target_id . "|" . $target_prof_url

        string[] dataPartition = responseText.Split("|");
        string unData = dataPartition[1].Trim();
        string unPfp = GameManager.rootURL + dataPartition[2].Trim();
        string targetUN = dataPartition[3].Trim();
        string targetPfp = GameManager.rootURL + dataPartition[4].Trim();

        Debug.Log("target url: " + targetPfp);
        Debug.Log("self url: " + unPfp);

        unText.text = unData;
        targetUNText.text = targetUN;

        StartCoroutine(downloadImageFromURL(unPfp, unPfpImage));
        StartCoroutine(downloadImageFromURL(targetPfp, targetPfpImage));
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

        webcam.Play();
        camMesh.material.SetTexture("_MainTex", webcam);

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

            //moved this line from the top of TakeSnap(), if anything breaks
            scls.doTargetAssignment(scls.getUsername(), 100);
        }
        else if (jsonResponse.Contains("Selfie Not Approved"))
        {
            responseText.text = "Verification failed, re-upload or try a different image.";
        }
        else
        {
            responseText.text = "Error parsing server response.";
        }

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
            Debug.Log("hello?");
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
