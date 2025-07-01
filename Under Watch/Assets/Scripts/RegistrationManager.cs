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
using static System.Net.Mime.MediaTypeNames;
using static OnlineMapsBingMapsElevation;

public class RegistrationManager : MonoBehaviour
{
    public TMP_InputField firstName;
    public TMP_InputField lastName;
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_InputField username;

    public RawImage profPic;
    public Texture2D defaultPfp;

    public static SC_LoginSystem loginSystem;
    public static GameManager gm;
    public SelfieCam selfieCam;

    public int socialFeedIndex;

    //public NativeGallery.MediaPickCallback ngmpc = new NativeGallery.MediaPickCallback(handleNewPicture);

    bool isWorking = false;

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

    public GameObject privacyPolicy;

    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public TextMeshProUGUI responseText;
    public string pfpPath;

    new List<Vector2> originalPos = new List<Vector2>();
    //hopefully this value prevents anything from going off the screen
    int goDownByValue = 150;
    public int pointsStart = 100;

    // Start is called before the first frame update
    public void Start()
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

            selfieCam.InitWebcam();
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
            responseText.text = "Missing one or more fields.";
        }
        else if (camUI.activeSelf)
        {
            selfieCam.InitWebcam();

            responseText.text = "";
            regTextFields.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => regTextFields.SetActive(false));
            nextButton.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => nextButton.SetActive(false));
            camUI.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad)
                .OnComplete(() => setCameraActive(true));
            camMeshObj.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad);
        }
        else
        {
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

    public void RegisterUser()
    {
        if (email.text == "" || username.text == "" || password.text == "" || firstName.text == "" || lastName.text == "")
        {
            responseText.text = "Missing one or more fields.";
        }
        else if (!isWorking)
        {
            StartCoroutine(doRegistration());
            responseText.text = "";
        }
    }

    public IEnumerator doRegistration()
    {
        Debug.Log("running");
        yield return frameEnd;

        isWorking = true;
        string errorMessage = "";

        WWWForm form = new WWWForm();

        Debug.Log(pfpPath);

        if (File.Exists(pfpPath))
        {
            string[] imageNames = pfpPath.Split("/");
            //string imageName = "testingPic.png";
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

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "register.php", form))
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

                Debug.Log("web result error" + www.error + www.result);
            }
            else
            {
                string responseStr = www.downloadHandler.text;
                Debug.Log("response" + responseStr);
                Debug.Log(username.text);
                if (responseStr.Contains("Success"))
                {
                    bg.transform.DOLocalMoveY(Screen.height * 3, .7f).SetEase(Ease.OutQuad);
                    canvasElement.transform.DOLocalMoveY(Screen.height * 3, .7f).SetEase(Ease.OutQuad);

                    loadingAnim.SetActive(false);
                    Debug.Log("successRegister");

                    loginSystem.loginUponRegister(username.text, email.text, pointsStart, password.text);
                    loginSystem.SetLoginPrefs(email.text, password.text, true);
                    privacyPolicy.SetActive(true);
                }
                else
                {
                    loadingAnim.SetActive(false);
                    regTextFields.SetActive(true);
                    regButton.SetActive(true);
                    pfpImage.SetActive(true);

                    errorMessage = responseStr;
                    responseText.text = errorMessage;
                    Debug.Log("error: " + errorMessage);
                }
            }
        }
        isWorking = false;
    }
    public void CapturedPhotoFinalStep(Texture2D pfp, string filepath)
    {
        profPic.texture = pfp;
        pfpPath = filepath;

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
    }
}
