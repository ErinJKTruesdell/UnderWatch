using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NativeGalleryNamespace;
using UnityEngine.Networking;
using System.Net;
using System.IO;
using static SC_LoginSystem;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using UnityEngine.AI;
using UnityEngine.Windows.WebCam;

public class RegistrationManager : MonoBehaviour
{
    public TMP_InputField firstName;
    public TMP_InputField lastName;
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_InputField username;

    public TMP_Text errorText;

    public RawImage profPic;
    public RawImage profPicOverlay;

    public SC_LoginSystem loginSystem;
    public ActivityStarter activityStarter;
    public GameManager gm;

    public int socialFeedIndex;

    //public NativeGallery.MediaPickCallback ngmpc = new NativeGallery.MediaPickCallback(handleNewPicture);

    bool isWorking = false;
    string rootURL = "https://erinjktruesdell.com/";

    bool profImageSet = false;
    public Toggle cacheCheckToggle;

    //camera stuff:
    public GameObject camUI;
    public GameObject regTextFields;
    public GameObject regAndPfp;
    public GameObject nextButton;
    public GameObject pfpImage;

    public RawImage rear;
    WebCamDevice[] devices;

    WebCamTexture webcam;
    public MeshRenderer camMesh;
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public GameObject testBox;

    public TextMeshProUGUI responseText;
    public string pfpPath;

    // Start is called before the first frame update
    void Start()
    {
        loginSystem = GameObject.FindObjectOfType<SC_LoginSystem>();

        if (loginSystem == null)
        {
            loginSystem = new SC_LoginSystem();
        }
        
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
    }
    public void BackButton()
    {
        if (camUI.activeSelf)
        {
            regTextFields.SetActive(true);
            camUI.SetActive(false);
            nextButton.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("LoginScene");
        }

    }

    public void NextToCamera()
    {
        if (email.text == "" || username.text == "" || password.text == "" || firstName.text == "" || lastName.text == "")
        {
            errorText.text = "Missing one or more fields.";
        }
        else
        {
            errorText.text = "";

            regTextFields.SetActive(false);
            nextButton.SetActive(false);
            camUI.SetActive(true);
            pfpImage.SetActive(false);
        }
    }

    /*public void SetProfilePicture()
    {
        NativeGallery.GetImageFromGallery(ngmpc, "Select Profile Image");
    }

    public static void handleNewPicture(string path)
    {
        picturePassthrough("file://" + path, GameObject.FindObjectOfType<RegistrationManager>());
    }

    public static void picturePassthrough(string path, RegistrationManager instance)
    {
        instance.StartCoroutine(instance.GetTex(path));
    }
    */
    public IEnumerator GetTex(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                while (texture.height > 1920 || texture.width > 1920)
                {
                    Debug.Log("Old: " + texture.width);
                    texture = ScaleTexture(texture, texture.width / 2, texture.height / 2);
                    Debug.Log("NEW: " + texture.width);
                }

                Rect sourceRect = new Rect(0, 0, 0, 0);
                // crop it
                if (texture.height > texture.width)
                {
                    float bottomCorner = (texture.height / 2) - (texture.width / 2);

                    sourceRect = new Rect(0, bottomCorner, texture.width, texture.width);
                }
                else
                {

                    float bottomCorner = (texture.width / 2) - (texture.height / 2);

                    sourceRect = new Rect(bottomCorner, 0, texture.height, texture.height);
                }

                int x = Mathf.FloorToInt(sourceRect.x);
                int y = Mathf.FloorToInt(sourceRect.y);
                int width = Mathf.FloorToInt(sourceRect.width);
                int height = Mathf.FloorToInt(sourceRect.height);

                Color[] pix = texture.GetPixels(x, y, width, height);
                Texture2D destTex = new Texture2D(width, height);
                destTex.SetPixels(pix);
                destTex.Apply();

                profPic.texture = destTex;

                //set img tex

                //shut off overlay
                profPicOverlay.color = Color.clear;
                profImageSet = true;

            }
        }
    }
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

    public void RegisterUser()
    {
        if (email.text == "" || username.text == "" || password.text == "" || firstName.text == "" || lastName.text == "")
        {
            errorText.text = "Missing one or more fields.";
        }
        else if (!isWorking)
        {
            errorText.text = "";
            StartCoroutine(doRegistration());
        }
    }

    public IEnumerator doRegistration()
    {
        yield return frameEnd;

        isWorking = true;
        string errorMessage = "";

        WWWForm form = new WWWForm();

        if (File.Exists(pfpPath))
        {
            string[] imageNames = pfpPath.Split("/");
            string imageName = imageNames[imageNames.Length - 1];
            form.AddBinaryData("file", File.ReadAllBytes(pfpPath), imageName);
        }

        else
        {
            Debug.Log("File does not exist");
        }

        form.AddField("firstName", firstName.text);
        form.AddField("lastName", lastName.text);
        form.AddField("email", email.text);
        form.AddField("username", username.text);
        form.AddField("password1", password.text);
        form.AddField("submit", "submit");

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "register.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
            }
            //else
            // {
            string responseText = www.downloadHandler.text;
            Debug.Log("response" + responseText);
            Debug.Log(username.text);
            if (responseText.StartsWith("Success"))
            {
                //activityStarter.setAlarms();
                StartCoroutine(loginSystem.doTargetAssignment(username.text, 1));
                //store registration information - em
                loginSystem.SetLoginPrefs(email.text, password.text, cacheCheckToggle.isOn);

                SceneManager.LoadScene("SocialFeed");
                Debug.Log("success");
            }
            else
            {
                errorMessage = responseText;
                errorText.text = errorMessage;
                Debug.Log("error: " + errorMessage);

            }
            //}
        }

        loginSystem.loginUponRegister(username.text, email.text);

        isWorking = false;
    }



    public void capturePhoto()
    {
        regAndPfp.SetActive(true);
        regTextFields.SetActive(true);
        camUI.SetActive(false); ;
        pfpImage.SetActive(true);

        Texture2D snap = new Texture2D(webcam.width, webcam.height);
        snap.SetPixels(webcam.GetPixels());
        snap.Apply();
        camMesh.material.SetTexture("_MainTex", snap);
        //byte[] bytes = snap.EncodeToPNG();
        webcam.Stop();

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
        string filename = username.text + "profPic.png";
        pfpPath = Application.persistentDataPath + filename;
        Debug.Log("--------------------------------------SAVING TO PATH--------------------------------------");
        Debug.Log(pfpPath);
        Debug.Log("------------------------------------------------------------------------------------------");
        System.IO.File.WriteAllBytes(pfpPath, bytes);

        StartCoroutine(GetTex(pfpPath));

    }
}
