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
using DG.Tweening;


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
    string rootURL = "egs01.westphal.drexel.edu/";

    bool profImageSet = false;
    public Toggle cacheCheckToggle;

    //camera stuff:
    public GameObject camUI;
    public GameObject regTextFields;
    public GameObject regButton;
    public GameObject nextButton;
    public GameObject pfpImage;

    public GameObject loadingAnim;
    public GameObject canvasElement;
    public GameObject bg;
    public GameObject camMeshObj;
    public GameObject cacheToggle;

    public RawImage rear;
    WebCamDevice[] devices;

    WebCamTexture webcam;
    public MeshRenderer camMesh;
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public GameObject testBox;

    public TextMeshProUGUI responseText;
    public string pfpPath;

    new List<Vector2> originalPos = new List<Vector2>();
    //hopefully this value prevents anything from going off the screen
    int goDownByValue = 150;

    // Start is called before the first frame update
    void Start()
    {
        regTextFields.transform.DOLocalMoveX(1400f, .5f).From().SetEase(Ease.OutQuad)
            .OnComplete(() => originalPos.Add(regTextFields.transform.localPosition)); 
        nextButton.transform.DOLocalMoveX(1400f, .5f).From().SetEase(Ease.OutQuad)
            .OnComplete(() => originalPos.Add(regButton.transform.localPosition));

        loginSystem = GameObject.FindObjectOfType<SC_LoginSystem>();

        if (loginSystem == null)
        {
            loginSystem = new SC_LoginSystem();
        }
        
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
                webcam = new WebCamTexture(devices[1].name);
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
    }
    public void BackButton()
    {
        if (camUI.activeSelf)
        {
            regTextFields.SetActive(true);
            nextButton.SetActive(true);

            regTextFields.transform.localPosition = new Vector2(-1400f, originalPos[0].y);
            nextButton.transform.localPosition = new Vector2(-1400f, originalPos[1].y);

            regTextFields.transform.DOLocalMoveX(originalPos[0].x, .5f).SetEase(Ease.OutQuad);
            nextButton.transform.DOLocalMoveX(originalPos[1].x, .5f).SetEase(Ease.OutQuad);
            camUI.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => setCameraActive(false));
            camMeshObj.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad);

        }
        else if (pfpImage.activeSelf)
        {
            regTextFields.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => regTextFields.SetActive(false));
            regButton.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => regButton.SetActive(false));
            pfpImage.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => pfpImage.SetActive(false));
            camUI.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => setCameraActive(true));
            camMeshObj.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad);

            webcam.Play();
            camMesh.material.SetTexture("_MainTex", webcam);
            pfpPath = "";
        }
        else
        {
            SceneManager.LoadScene("LoginScene");
            regTextFields.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad);
            nextButton.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad);
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
            webcam.Play();
            camMesh.material.SetTexture("_MainTex", webcam);
            pfpPath = "";

            errorText.text = "";
            regTextFields.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => regTextFields.SetActive(false));
            nextButton.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => nextButton.SetActive(false));
            camUI.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => setCameraActive(true));
            camMeshObj.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad);
        }
    }

    void setCameraActive(bool active)
    {
        camUI.SetActive(active);
        camMeshObj.SetActive(active);
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

            loadingAnim.SetActive(true);
            regTextFields.SetActive(false);
            regButton.SetActive(false);
            pfpImage.SetActive(false);

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
                loadingAnim.SetActive(false);
                regTextFields.SetActive(true);
                regButton.SetActive(true);
                pfpImage.SetActive(true);

            }
            //else
            // {
            string responseText = www.downloadHandler.text;
            Debug.Log("response" + responseText);
            Debug.Log(username.text);
            if (responseText.StartsWith("Success"))
            {
                activityStarter.setAlarms();
                StartCoroutine(loginSystem.doTargetAssignment(username.text, 1));
                //store registration information - em
                loginSystem.SetLoginPrefs(email.text, password.text, cacheCheckToggle.isOn);

                bg.transform.DOLocalMoveY(Screen.height * 3, .7f).SetEase(Ease.OutQuad);
                canvasElement.transform.DOLocalMoveY(Screen.height * 3, .7f).SetEase(Ease.OutQuad).OnComplete(() => gm.ProgressToScene("SocialFeed"));
                loadingAnim.SetActive(false);
                Debug.Log("success");
            }
            else
            {
                loadingAnim.SetActive(false);
                regTextFields.SetActive(true);
                regButton.SetActive(true);
                pfpImage.SetActive(true);

                errorMessage = responseText;
                errorText.text = errorMessage;
                Debug.Log("error: " + errorMessage);

            }
            //}
        }

        loginSystem.loginUponRegister(username.text, email.text);

        isWorking = false;
    }

    public void startPhotoCoroutine()
    {
        StartCoroutine(capturePhoto());
    }

    public IEnumerator capturePhoto()
    {
        if (devices.Length > 1)
        {
            yield return frameEnd;

            Texture2D snap = new Texture2D(webcam.width, webcam.height);
            snap.SetPixels(webcam.GetPixels());
            snap.Apply();
            camMesh.material.SetTexture("_MainTex", snap);
            //byte[] bytes = snap.EncodeToPNG();
            webcam.Stop();

            Vector3[] corners = new Vector3[4];
            rear.rectTransform.GetWorldCorners(corners);
            Debug.Log(corners);
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
            Debug.Log("PATH: " + pfpPath);
            Debug.Log("------------------------------------------------------------------------------------------");
            System.IO.File.WriteAllBytes(pfpPath, bytes);

            regTextFields.SetActive(true);
            nextButton.SetActive(false);
            regButton.SetActive(true);
            pfpImage.SetActive(true);

            regTextFields.transform.localPosition = new Vector2(1400, originalPos[0].y - goDownByValue);
            regButton.transform.localPosition = new Vector2(1400, originalPos[1].y - goDownByValue);
            pfpImage.transform.localPosition = new Vector2(1400, pfpImage.transform.localPosition.y);

            regTextFields.transform.DOLocalMoveX(originalPos[0].x, .5f).SetEase(Ease.OutQuad);
            regButton.transform.DOLocalMoveX(originalPos[1].x, .5f).SetEase(Ease.OutQuad);
            pfpImage.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad);

            camUI.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => setCameraActive(false));
            camMeshObj.transform.DOLocalMoveX(-1400, .5f).SetEase(Ease.OutQuad);

            StartCoroutine(GetTex(pfpPath));
        }
        else
        {
            responseText.color = Color.red;
            responseText.text = "No camera detected";
        }
    }
}
