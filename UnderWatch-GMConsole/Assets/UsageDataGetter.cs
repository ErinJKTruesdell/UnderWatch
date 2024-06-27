using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;



public class socialTimeData
{
    public string userID;
    public string username;
    public float socialTime;
    public float totalTime;
    public socialTimeData(string id, string un, string social, string total)
    {
        userID = id;  username = un; socialTime = float.Parse(social); totalTime = float.Parse(total);
    }
}


public class UsageDataGetter : MonoBehaviour
{
    public GameObject leaderboardItemPrefab;
    public List<GameObject> usageItems = new List<GameObject>();

    string rootURL = "https://erinjktruesdell.com/";

    public TMP_Text avgTotalText;
    public TMP_Text avgSocialText;

    public Transform gridObj;

    public void doUsageData()
    {
        foreach(GameObject g in  usageItems)
        {
            Destroy(g);
        }

        usageItems = new List<GameObject>();
        StartCoroutine(StockLeaderBoard());
    }

    IEnumerator StockLeaderBoard()
    {
        // get data from server
        WWWForm form = new WWWForm();
        form.AddField("s", "s"); //dummy data

        List<socialTimeData> allTimes = new List<socialTimeData>();

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/get-social-times.php", form))
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

                Debug.Log(responseText);
                string[] userChunks = responseText.Split('%');
                foreach (string chunk in userChunks)
                {
                    string[] userData = chunk.Split('|');
                    if (userData.Length > 3) // trim off that last empty bit
                    {
                        //user id, username, social time, total time
                        //Debug.Log(userData[0] + ": " + userData[1] + " points |" + userData[2]);
                        allTimes.Add(new socialTimeData(userData[0], userData[1], userData[2], userData[3]));
             

                    }
                }


                allTimes.Sort((x, y) => x.totalTime.CompareTo(y.totalTime));
                allTimes.Reverse();
                gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, allTimes.Count * 70);


                float totalAppTimeSpent = 0f;
                float totalSocialTimeSpent = 0f;
                float totalUsersListed = (float)allTimes.Count;



                foreach (socialTimeData i in allTimes)
                {

                    GameObject leaderBoardUserItem = Instantiate(leaderboardItemPrefab) as GameObject;
                    leaderBoardUserItem.transform.parent = gridObj;
                    UsageItem li = leaderBoardUserItem.GetComponent<UsageItem>();
                    li.setStats(i.userID, i.username, i.socialTime, i.totalTime);
                    usageItems.Add(leaderBoardUserItem);
                    totalAppTimeSpent += i.totalTime;
                    totalSocialTimeSpent += i.socialTime;
                   // li.usernameText.text = i.username;
                   // li.pointsText.text = i.points.ToString() + " points";

                    //downlaod prof img
                }

                float avgApp = totalAppTimeSpent / (float)totalUsersListed;
                float avgSocial = totalSocialTimeSpent / (float)totalUsersListed;
                avgTotalText.text = avgApp.ToString("n2");
                avgSocialText.text = avgSocial.ToString("n2");

            }
            //break data into chunks

            //iterate through chunks, downloading pfolie pics and creating prefabs


            //close loading screen, if one implemented

        }
    }
}
