using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileUpdateManager : MonoBehaviour
{


    public TMP_Text errorText;

    public RawImage profPic;
    public RawImage profPicOverlay;
    SC_LoginSystem loginSystem;

   // public int socialFeedIndex;

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

    public void SetProfilePicture()
    {
        NativeGallery.GetImageFromGallery(ngmpc, "Select Profile Image");
    }

    public static void handleNewPicture(string path)
    {
        picturePassthrough("file://" + path, GameObject.FindObjectOfType<ProfileUpdateManager>());
    }

    public static void picturePassthrough(string path, ProfileUpdateManager instance)
    {
        instance.StartCoroutine(instance.GetTex(path));
    }

    public IEnumerator GetTex(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                errorText.text = uwr.error;
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
        
        if (!isWorking && profImageSet)
        {
            errorText.text = "";
            StartCoroutine(doUpdate());
        }

    }

    public IEnumerator doUpdate()
    {
        isWorking = true;
        string errorMessage = "";

        WWWForm form = new WWWForm();
        string un = loginSystem.getUsername();
        form.AddField("username", un);
        Debug.Log(un);
        if (profImageSet)
        {
            form.AddBinaryData("file", ImageConversion.EncodeToPNG(((Texture2D)profPic.texture)), loginSystem.getUsername() + "profPic.png");
        }


        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "profPic.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
            }
            //else
            // {
            string responseText = www.downloadHandler.text;
            Debug.Log(responseText);
            if (responseText.StartsWith("Success"))
            {
                SceneManager.LoadScene("PlayerProfile");
            }
            else
            {
                errorMessage = responseText;
                errorText.text = errorMessage;
            }
            //}
        }

        isWorking = false;
    }
}
