using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NativeGalleryNamespace;
using UnityEngine.Networking;
using static SC_LoginSystem;
using UnityEngine.SceneManagement;

public class ProfileUpdateManager : MonoBehaviour
{
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_InputField username;



    public TMP_Text errorText;

    public RawImage profPic;
    public RawImage profPicOverlay;

    SC_LoginSystem loginSystem;

    public int socialFeedIndex;

    public NativeGallery.MediaPickCallback ngmpc = new NativeGallery.MediaPickCallback(handleNewPicture);

    bool isWorking = false;
    string rootURL = "https://erinjktruesdell.com/";

    bool profImageSet = false;

    public GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindObjectOfType<GameManager>();

        if (gm == null)
        {
            gm = new GameManager();
        }

        loginSystem = gm.scls;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetProfilePicture()
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

    public void UpdatePicture()
    {
        StartCoroutine(DoUpdatePicture());
    }

    public IEnumerator DoUpdatePicture()
    {
        isWorking = true;
        string errorMessage = "";

        WWWForm form = new WWWForm();
        form.AddField("email", email.text);

        form.AddField("submit", "submit");

        if (profImageSet)
        {
            form.AddBinaryData("file", ImageConversion.EncodeToPNG(((Texture2D)profPic.texture)), username.text + "profPic.png");
        }


        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "updateProfile.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
            }

            string responseText = www.downloadHandler.text;
            Debug.Log(responseText);
            if (responseText.StartsWith("Success"))
            {
                gm.ProgressToScene("SocialFeed"); //TODO this should go to a "profile" page eventually. once it exists
            }
            else
            {
                errorMessage = responseText;
                errorText.text = errorMessage;
            }
        }

        isWorking = false;
    }

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

                    sourceRect = new Rect(bottomCorner, 0, texture.width, texture.width);
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

    //public void RegisterUser()
    //{
    //    if (email.text == "" || username.text == "" || password.text == "")
    //    {
    //        errorText.text = "Missing one or more fields.";
    //    }
    //    else if (!isWorking)
    //    {
    //        errorText.text = "";
    //        StartCoroutine(doRegistration());
    //    }

    //}

    //public IEnumerator doRegistration()
    //{
    //    isWorking = true;
    //    string errorMessage = "";

    //    WWWForm form = new WWWForm();
    //    form.AddField("email", email.text);
    //    form.AddField("username", username.text);
    //    form.AddField("password1", password.text);
    //    form.AddField("submit", "submit");
    //    if (profImageSet)
    //    {
    //        form.AddBinaryData("file", ImageConversion.EncodeToPNG(((Texture2D)profPic.texture)), username.text + "profPic.png");
    //    }


    //    using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "register.php", form))
    //    {
    //        yield return www.SendWebRequest();

    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            errorMessage = www.error;
    //        }
    //        //else
    //        // {
    //        string responseText = www.downloadHandler.text;
    //        Debug.Log(responseText);
    //        if (responseText.StartsWith("Success"))
    //        {
    //            SceneManager.LoadScene(socialFeedIndex);
    //        }
    //        else
    //        {
    //            errorMessage = responseText;
    //            errorText.text = errorMessage;
    //        }
    //        //}
    //    }

    //    isWorking = false;
    //}
}
