using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileDatabase : MonoBehaviour
{
    public RawImage profileImage;

    public RawImage zoomedImage;
    public GameObject zoomedImageObject;

    public GameObject photoPrefab; // has RawImage component that holds actual image
    public Transform contentTransform;

    public TMP_Text usernameText;
    public TMP_Text fullNameText;

    public GameManager gm;

    private void Start()
    {
        gm = GameObject.FindObjectOfType<GameManager>();
    }
    public void PfDLogOut()
    {
        gm.LogOut();
    }

    void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "PlayerProfile")
        {
            fillCanvas(GameManager.loggedInUser);
        }
    }

    public void fillCanvas(UserInfo user)
    {
        Debug.Log("Filling Canvas: " + user.un);

        usernameText.text = "@" + user.un;
        profileImage.texture = user.profilePic;
        usernameText.gameObject.SetActive(true);

        StartCoroutine(getAndDownloadImages(user.un));
        fullNameText.text = user.firstName + " " + user.lastName;
    }
    private IEnumerator getAndDownloadImages(string username)
    {

        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        // get data from server
        WWWForm form = new WWWForm();
        form.AddField("username", username); //dummy data

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-all-user-photos.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                string errorMessage = www.error;
                Debug.Log(errorMessage);

                Debug.Log("Data get error, releasing queue");
            }
            else
            {
                //get response
                string responseText = www.downloadHandler.text;
                Debug.Log("profile: " + responseText);
                string[] userChunks = responseText.Split('|');

                //resize content
                contentTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(750, Mathf.Floor(userChunks.Length/3f) * 250);

                if (userChunks.Length > 1)
                {
                    fullNameText.text = userChunks[0];

                    string profUrl = "/" + userChunks[1];

                    //create prefab and load images
                    for (int s = 1; s < userChunks.Length; s++)
                    {
                        string i = userChunks[s];
                        if (i != "")
                        {
                            GameObject picItem = Instantiate(photoPrefab) as GameObject;
                            picItem.transform.SetParent(contentTransform, false);

                            PlayerPhotoProfile ppp = picItem.GetComponent<PlayerPhotoProfile>();
                            ppp.zoomedImage = zoomedImage;
                            ppp.zoomedImageObj = zoomedImageObject;
                            //downlaod prof img
                            StartCoroutine(downloadImageFromURL(GameManager.rootURL + i, ppp.thisImage));
                        }
                    }
                }
            }
        }
    }

    private IEnumerator downloadImageFromURL(string url1, RawImage image1)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url1);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            image1.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }

    }
}
