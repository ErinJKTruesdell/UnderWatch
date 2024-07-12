using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static OnlineMapsGPXObject;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AdUploadManager : MonoBehaviour
{

    string[] filepaths;
    public TMP_Text errorText;
    public AdTimestampManager atm;
    bool isWorking;

    string rootURL = "https://erinjktruesdell.com/";


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
