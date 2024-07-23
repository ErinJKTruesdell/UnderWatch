using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using TMPro;
using SimpleFileBrowser;
using UnityEngine.UI;
using System;

public class ConfederateUploader : MonoBehaviour
{


    public string uploadURL = "https://erinjktruesdell.com/uploadImage.php";
    public string confederateUN = "rickastley69420";

    public TMP_Text statustext;

    public string[] toUpload;

    public Button uploadButton;

    public ConfTimestamper cts;

    // Start is called before the first frame update
    void Start()
    {
        uploadButton.interactable = false;
    }

    public void doSelect()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    // Update is called once per frame
    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        FileBrowser.SetFilters(false, new string[] { ".png" });
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select Files", "Load");

        // Dialog is closed
        // Print whether the user has selected some files or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result); // FileBrowser.Result is null, if FileBrowser.Success is false
    }

    void OnFilesSelected(string[] filePaths)
    {
        toUpload = filePaths;
        if (filePaths.Length > 0)
        {
            uploadButton.interactable = true;
        }
    }

    public void onUploadButtonPress()
    {
        StartCoroutine(uploadFile(toUpload[0]));
    }

    public IEnumerator uploadFile(string filePath)
    {
        statustext.text = ""; //reset
        string loggedInUser = confederateUN;
        Debug.Log(filePath);


        WWWForm form = new WWWForm();
        string[] imageNames = filePath.Split("/");
        string imageName = imageNames[imageNames.Length - 1];
        form.AddBinaryData("file", File.ReadAllBytes(filePath), imageName);
        form.AddField("username", loggedInUser);
        Tuple<int, int, int> time = cts.selectedTimestamp();
        form.AddField("month", time.Item1);
        form.AddField("day",time.Item2);
        form.AddField("hour", time.Item3);

        UnityWebRequest www = UnityWebRequest.Post(uploadURL, form);
        Debug.Log("Sending web request...");

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            statustext.text = www.error;
        }
        else
        {
            statustext.text = "File successfully uploaded to confederate account.";
            toUpload = new string[0];
            uploadButton.interactable = false;


        }




    }
}
