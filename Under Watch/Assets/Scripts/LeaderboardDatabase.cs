using Mopsicus.InfiniteScroll;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class  pointsData
{
    public string username;
    public int points;
    public string profUrl;
    public pointsData(string un, string pts, string url)
    {
        username = un; points = int.Parse(pts); profUrl = url;
    }
}
public class LeaderboardDatabase : MonoBehaviour
{


    public GameObject leaderboardItemPrefab;

    public GameObject leaderboardloading;

    string rootURL = "egs01.westphal.drexel.edu/";

    bool isWorking;

    public Transform gridObj;

    void Start()
    {


    }

    private void Awake()
    {
        if (leaderboardloading != null)
        {
            leaderboardloading.SetActive(true);
        }
        StartCoroutine(StockLeaderBoard());
    }

    IEnumerator StockLeaderBoard()
    {
        // get data from server
        WWWForm form = new WWWForm();
        form.AddField("d", "d"); //dummy data

        List<pointsData> allPoints = new List<pointsData>();

        using (UnityWebRequest www = UnityWebRequest.Post("https://egs01.westphal.drexel.edu/get-next-leaderboard.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
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
                string[] userChunks = responseText.Split('%');
                foreach (string chunk in userChunks)
                {
                    string[] userData = chunk.Split('|');
                    if (userData.Length > 2) // trim off that last empty bit
                    {
                        //username, points value, prof URL
                        //Debug.Log(userData[0] + ": " + userData[1] + " points |" + userData[2]);
                        allPoints.Add(new pointsData(userData[0], userData[1], userData[2]));

                    }
                }


                allPoints.Sort((x, y) => x.points.CompareTo(y.points));
                allPoints.Reverse();

                gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, allPoints.Count * 200);

                foreach(pointsData i in allPoints)
                {

                    GameObject leaderBoardUserItem = Instantiate(leaderboardItemPrefab) as GameObject;
                    leaderBoardUserItem.transform.parent = gridObj;
                    leaderBoardUserItem.transform.localScale = new Vector3(1, 1, 1);


                    LeaderboardItem li = leaderBoardUserItem.GetComponent<LeaderboardItem>();

                    li.usernameText.text = i.username.Trim();
                    li.pointsText.text = i.points.ToString() + " points";

                    //downlaod prof img
                    StartCoroutine(downloadImageFromURL(rootURL + i.profUrl, li.profilePic));
                }

            }
            //break data into chunks

            //iterate through chunks, downloading pfolie pics and creating prefabs


            //close loading screen, if one implemented
            if (leaderboardloading != null)
            {
                leaderboardloading.SetActive(false);
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

}
