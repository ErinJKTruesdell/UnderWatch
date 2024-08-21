using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using SFB; // Standalone File Browser namespace

public class ApiClient : MonoBehaviour
{
   // public Button selectButton;
    //public Button uploadButton;
    public TextMeshProUGUI responseText;
    public string rootURL = "https://erinjktruesdell.com/";
    public GameObject blockingPanel;
    public GameObject closeButton;

    //public RawImage uploadedImage;

    //private string selectedFilePath;
    //private string userId = "user1"; // Hardcoded user ID for demonstration

    /*void Start()
    {
        selectButton.onClick.AddListener(OnSelectButtonClicked);
        uploadButton.onClick.AddListener(OnUploadButtonClicked);
        uploadButton.interactable = false;
    }

    void OnSelectButtonClicked()
    {
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" )
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

        if (paths.Length > 0)
        {
            selectedFilePath = paths[0];
            responseText.text = "Selected: " + Path.GetFileName(selectedFilePath);
            uploadButton.interactable = true;
            DisplaySelectedImage(selectedFilePath);
        }
    }

    void DisplaySelectedImage(string filePath)
    {
        byte[] imageBytes = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        uploadedImage.texture = texture;
        uploadedImage.rectTransform.sizeDelta = new Vector2(512, 512);
    }

    void OnUploadButtonClicked()
    {
        if (!string.IsNullOrEmpty(selectedFilePath))
        {
            StartCoroutine(UploadImage(selectedFilePath, userId));
        }
    }*/

    public IEnumerator UploadImage(string filePath, string userId)
    {
        responseText.text = "Verifying Image...";
        StartCoroutine(ShowProcessingAnimation());
        blockingPanel.SetActive(true);

        byte[] imageBytes = File.ReadAllBytes(filePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageBytes, Path.GetFileName(filePath), "image/jpeg");
        form.AddField("user_id", userId);

        Debug.Log("Uploading image to server");

        using (UnityWebRequest webRequest = UnityWebRequest.Post(rootURL + "/uploadImage.php", form)) // Adjust URL as necessary
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(": Error: " + webRequest.error);
                responseText.text = "Error: " + webRequest.error;

                StopAllCoroutines(); // Stop the processing animation
                closeButton.SetActive(true);
            }
            else
            {
                Debug.Log(": Received: " + webRequest.downloadHandler.text);
                HandleServerResponse(webRequest.downloadHandler.text);
                closeButton.SetActive(true);
            }
        }
    }

    public void CloseBlockerPanel()
    {
        blockingPanel.SetActive(false);
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
        Debug.Log(jsonResponse + "json");

        if (jsonResponse.Contains("Selfie Approved"))
        {
            responseText.text = "Verification successful";
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

    [System.Serializable]
    private class ServerResponse
    {
        public bool match;
    }
}
