using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mopsicus.InfiniteScroll;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Net;
using System.Xml.Linq;
using UnityEngine.SceneManagement;

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
    
    public GameObject SearchBar;

    public GameObject searchProfileItem;

    public GameObject loading;

    string rootURL = "https://erinjktruesdell.com/";

    public Transform gridObj;

    public List<GameObject> Element = new();

    public static string prevSearches;

    private void Awake()
    {
        if (loading != null)
        {
            loading.SetActive(true);
        }


        StartCoroutine(PopulateSearch());

        prevSearches = PlayerPrefs.GetString("SearchedUNs");
    }

    public void Start()
    {
        Debug.Log("stat");
        Debug.Log("Saved search:" + SearchScript.prevSearches);
        //todo: figure out why the saving is inconsistent AF
        foreach (GameObject ele in Element)
        {
            if (prevSearches.Contains(ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.ToLower()))
            {
                ele.SetActive(true);
                Debug.Log("Contains: " + ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Clearing cached searches");
            PlayerPrefs.DeleteKey("SearchedUNs");
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
            }
            else
            {
                //return null
                string responseText = www.downloadHandler.text;
                Debug.Log("search response:" + responseText);
                //this should two chunks: usernames, then pfps 
                string[] dataPartition = responseText.Split("@");
                string[] UNData = dataPartition[0].Split('|');
                string[] PFPData = dataPartition[1].Split('|');

                for (int j = 0; j < UNData.Length; j++)
                {
                    if (UNData[j].Length > 2) // trim off that last empty bit
                    {
                        //Debug.Log(PFPData[j]);
                        userNames.Add(new userData(UNData[j], PFPData[j]));
                    }
                }

                //sort list alphabetically
                userNames.Sort((x, y) => x.username.CompareTo(y.username));

                gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, userNames.Count * 200);

                foreach (userData i in userNames)
                {

                    GameObject searchUserItem = Instantiate(searchProfileItem) as GameObject;
                    searchUserItem.transform.parent = gridObj;
                    searchUserItem.transform.localScale = new Vector3(1, 1, 1);

                    Element.Add(searchUserItem);

                    searchListItem li = searchUserItem.GetComponent<searchListItem>();

                    li.usernameText.text = i.username.Trim();
                    if (!prevSearches.Contains(i.username.Trim()))
                    {
                        searchUserItem.SetActive(false);
                    }


                    //downlaod prof img
                    StartCoroutine(downloadImageFromURL(i.profUrl, li.profilePic));
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
            
            if (ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Length >= searchTxtLength)//and if the search text is greater than 0
            {
                if (SearchText.ToLower() == ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Substring(0, searchTxtLength).ToLower())
                {
                    ele.SetActive(true);
                    //scroll to top
                }
                else
                {
                    if (!prevSearches.Contains(ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text))
                    {
                        //this...shouldn't work but it really does
                        ele.SetActive(false);
                        //Debug.Log("Contains: " + ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
                    }
                    //Debug.Log("ele:" + ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
                }
            }      
        }
    }
    
}
