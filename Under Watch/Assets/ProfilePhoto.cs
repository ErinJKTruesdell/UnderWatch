/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class ProfilePhoto : MonoBehaviour
{
    public RawImage rear;
    WebCamDevice[] devices;

    WebCamTexture webcam;

    public string uploadURL = "egs01.westphal.drexel.edu/uploadImage.php";

    public MeshRenderer camMesh;
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public GameManager gm;
    public SC_LoginSystem scls;

    public GameObject testBox;

    public TextMeshProUGUI responseText;
    public TextMeshProUGUI usernameText;

    Vector3 currentLocalEurlerAngles = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {

        devices = WebCamTexture.devices;
        WebCamDevice frontCamera;
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
            webcam = new WebCamTexture(devices[1].name);
        }

        webcam.Play();
        camMesh.material.SetTexture("_MainTex", webcam);

        gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            gm = new GameManager();
        }
        scls = gm.scls;
    }
    public IEnumerator takeSnap()
    {
        yield return frameEnd;

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
        string filename = usernameText.text + "profPic.png";
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

                UnityWebRequest www = UnityWebRequest.Post(uploadURL, form);
                Debug.Log("Sending web request...");

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    responseText.text = "Error: " + www.error;

                    Debug.Log(www.error);
                    StopAllCoroutines(); // Stop the processing animation
                    closeButton.SetActive(true);
                }
                else
                {
                    Debug.Log("Form upload complete! " + System.Text.Encoding.ASCII.GetString(www.downloadHandler.data));
                    HandleServerResponse(www.downloadHandler.text);
                    closeButton.SetActive(true);

                }
            }
            else
            {
                Debug.Log("File does not exist");
            }

        }

    }

    public void capturePhoto()
    {
        Texture2D snap = new Texture2D(webcam.width, webcam.height);
        snap.SetPixels(webcam.GetPixels());
        snap.Apply();
        camMesh.material.SetTexture("_MainTex", snap);
        //byte[] bytes = snap.EncodeToPNG();
        webcam.Stop();

        StartCoroutine(takeSnap());

    }
}
*/