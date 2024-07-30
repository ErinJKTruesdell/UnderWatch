using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mopsicus.InfiniteScroll;
using UnityEngine.Networking;
using UnityEngine.UI;

public class userData
{
    public string username;
    public string profUrl;

    public userData(string un, string url)
    {
        username = un; profUrl = url;
    }
}

public class SearchScript : MonoBehaviour
{
    public GameObject ContentHolder;
    //change to a list for infinite space
    public GameObject[] Element;

    public GameObject SearchBar;

    public int totalElements;

    public GameObject searchProfileItem;

    public GameObject loading;

    string rootURL = "https://erinjktruesdell.com/";

    bool isWorking;

    public Transform gridObj;

    private void Awake()
    {
        if (loading != null)
        {
            loading.SetActive(true);

            StartCoroutine(PopulateSearch());
        }
    }

    void Start()
    {
        totalElements = ContentHolder.transform.childCount;

        Element = new GameObject[totalElements];

        for (int i = 0; i < totalElements; i++)
        {
            Element[i] = ContentHolder.transform.GetChild(i).gameObject;
        }
    }

    IEnumerator PopulateSearch()
    {
        // get data from server
        WWWForm form = new WWWForm();
        form.AddField("s", "s"); //dummy data

        List<userData> userNames = new List<userData>();

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "get-usernames.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //yay show the picture
                //errorMessage = www.error;
                string errorMessage = www.error;
                Debug.Log(errorMessage);

                Debug.Log("Data get error, releasing queue");
                isWorking = false;
            }
            else
            {
                //return null
                string responseText = www.downloadHandler.text;
                Debug.Log(responseText);
                //this should two chunks: usernames, then pfps 
                string[] dataPartition = responseText.Split("@");
                string[] UNData = dataPartition[0].Split('|');
                string[] PFPData = dataPartition[1].Split('|');

            foreach (string chunk in UNData)
            {
                userNames.Add(new userData(UNData[0], PFPData[0]));
            }
                Debug.Log(dataPartition);

                //sort list alphabetically
                gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, userNames.Count * 200);

                foreach (userData i in userNames)
                {

                    GameObject searchUserItem = Instantiate(searchProfileItem) as GameObject;
                    searchUserItem.transform.parent = gridObj;
                    searchListItem li = searchUserItem.GetComponent<searchListItem>();

                    li.usernameText.text = i.username;

                    //downlaod prof img
                    StartCoroutine(downloadImageFromURL(rootURL + i.profUrl, li.profilePic));
                }

            }
            //break data into chunks
            //iterate through chunks, downloading pfolie pics and creating prefabs

            //close loading screen, if one implemented
            if (loading != null)
            {
                loading.SetActive(false);
            }
        }
    }

    IEnumerator downloadImageFromURL(string url1, RawImage image1)
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

    public void Search()
    {
        string SearchText = SearchBar.GetComponent<TMP_InputField>().text;
        int searchTxtLength = SearchText.Length;

        int searchedElements = 0;

        foreach (GameObject ele in Element)
        {
            searchedElements++;

            if (ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Length >= searchTxtLength)
            {
                if (SearchText.ToLower() == ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Substring(0, searchTxtLength).ToLower())
                {
                    ele.SetActive(true);
                    //scroll to top
                }
                else
                {
                    ele.SetActive(false);
                }
            }
        }
    }
    
}
