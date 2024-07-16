using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static OnlineMapsGPXObject;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using System.IO;

public class AdUploadManager : MonoBehaviour
{

    public TabButton adUploadTab;

    string[] filepaths;
    public TMP_Text errorText;
    public AdTimestampManager atm;
    bool isWorking;

    public TMP_Text adRateText;
    public TMP_Dropdown adRateDropdown;

    public TMP_Text numadstext;

    string rootURL = "https://erinjktruesdell.com/";


    // Start is called before the first frame update
    void Start()
    {
        if (adUploadTab != null)
        {
            adUploadTab.Activated += new TabButton.ActivatedHandler(OnAdUploadActivated);
        }
    }

    // Update is called once per frame
    public void startAdSelect()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        FileBrowser.SetFilters(false, new string[] { ".png" });
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load");

        // Dialog is closed
        // Print whether the user has selected some files or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result); // FileBrowser.Result is null, if FileBrowser.Success is false
    }

    void OnFilesSelected(string[] filePaths)
    {
        filepaths = filePaths;
        numadstext.text = filePaths.Length + " ad files selected.";
    }
    public void OnAdUploadActivated(TabButton t, EventArgs e)
    {
        refreshAdRate();
    }

    public void startUpload()
    {
        if (!isWorking)
        {

            //get filepaths, then do all uploads

            StartCoroutine(uploadAllAds());

        }
        else
        {
            errorText.text = "Upload or ad selection already in progress.";
        }
    }

    public void SelectAds()
    {
        if (!isWorking)
        {
            StartCoroutine(doAdSelect());
        }
        else
        {
            errorText.text = "Upload or ad selection already in progress.";
        }
    }

    IEnumerator doAdSelect()
    {
        isWorking = true;
        yield return null;
        isWorking = false;
    }

    public void refreshAdRate()
    {
        StartCoroutine(getAdRate());


    }

    public IEnumerator setAdRate() {

        int selectedAdRate = adRateDropdown.value;
        // get data from server
        WWWForm form = new WWWForm();
        Debug.Log("Selected ad rate to upload: " + selectedAdRate);
        form.AddField("adRate", selectedAdRate.ToString()); 


        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/set_ad_rate.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                //errorMessage = www.error;
                string errorMessage = www.error;
                Debug.Log(errorMessage);
            }
            else
            {
                //return null
                string responseText = www.downloadHandler.text;


                Debug.Log("Response: " + responseText);
                selectedAdRate = int.Parse(responseText);
                adRateText.text = "Ads are currently shown every " + selectedAdRate.ToString() + " posts";


            }
        }

    }

    public IEnumerator getAdRate()
    {
        //get from server

        int selectedAdRate = -1;
        // get data from server
        WWWForm form = new WWWForm();
        form.AddField("s", "s"); //dummy data


        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/get_ad_rate.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                //errorMessage = www.error;
                string errorMessage = www.error;
                Debug.Log(errorMessage);
            }
            else
            {
                //return null
                string responseText = www.downloadHandler.text;
                Debug.Log("Response: " + responseText);
                selectedAdRate = int.Parse(responseText);



            }
        }

        adRateDropdown.ClearOptions();
        List<string> adRateOptions = new List<string>();
        for (int i = 0; i < 21; i++)
        {
            adRateOptions.Add(i.ToString());
        }
        adRateDropdown.AddOptions(adRateOptions);
        adRateDropdown.value = selectedAdRate;
        adRateText.text = "Ads are currently shown every " + selectedAdRate.ToString() + " posts";
    }

    public void sendUpdatedAdRate()
    {
        StartCoroutine(setAdRate());
    }

    public IEnumerator uploadAllAds()
    {

        foreach (string s in filepaths)
        {
            string[] filenameParts = s.Split("\\");
            string filename = filenameParts[filenameParts.Length - 1];


            string[] filenameImageParts = filename.Split("-");
            string uid = filenameImageParts[filenameImageParts.Length - 1].Split(".")[0];//the username is in this string

            //create WWform with username and image to upload, plus set timestamp (month/day) year can be appended automatically

            Tuple<int, int> monthDay = atm.selectedTimestamp();

            isWorking = true;
            string errorMessage = "";

            WWWForm form = new WWWForm();
            form.AddField("username", uid);
            form.AddField("month", monthDay.Item1.ToString());
            form.AddField("day", monthDay.Item2.ToString());

            byte[] FileUpload = null;
            bool isError = false;
            using (var www = UnityWebRequestTexture.GetTexture(s))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    errorText.text += "\nError opening file " + filename + ": " + errorMessage;
                    isError = true;
                }
                else
                {
                    // file data successfully loaded
                    FileUpload = www.downloadHandler.data;
                }
            }
            if (!isError)
            {
                form.AddBinaryData("file", FileUpload, "AD-" + "username" + DateTime.Now.ToString() + ".png", "image/png");

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
                    Debug.Log(responseText);
                    if (responseText.StartsWith("Success"))
                    {
                        SceneManager.LoadScene(5);
                    }
                    else
                    {
                        errorMessage = responseText;
                        errorText.text += "\nError uploading file " + filename + ": " + errorMessage;
                    }
                    //}
                }
            }


            isWorking = false;

            //if error, add to  error text, but keep going


        }

        yield return null;
    }
}
