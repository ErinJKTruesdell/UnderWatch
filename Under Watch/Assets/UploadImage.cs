using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using static SC_LoginSystem;

public class UploadImage : MonoBehaviour
{

    public string uploadURL = "https://erinjktruesdell.com/uploadImage.php";
    public SC_LoginSystem loginSystem;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void uploadIt(string path)
    {
        StartCoroutine(uploadFile(path));
    }

    public IEnumerator uploadFile(string filePath)
    {
        if (loginSystem != null && loginSystem.getIsLoggedIn())
        {
            string loggedInUser = loginSystem.getUsername();
            Debug.Log(filePath);

            if (File.Exists(filePath))
            {
                WWWForm form = new WWWForm();
                string[] imageNames = filePath.Split("/");
                string imageName = imageNames[imageNames.Length - 1];
                form.AddBinaryData("file", File.ReadAllBytes(filePath), imageName);
                form.AddField("username", loggedInUser);

                UnityWebRequest www = UnityWebRequest.Post(uploadURL, form);

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Form upload complete! " + System.Text.Encoding.ASCII.GetString(www.downloadHandler.data));

                }
            }
            else
            {
                Debug.Log("File does not exist");
            }

        }

    }
}
