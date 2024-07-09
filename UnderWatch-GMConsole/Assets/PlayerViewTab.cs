using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerViewTab : MonoBehaviour
{

    public TMP_Dropdown playerDropdown;
    public TabButton playerViewButton;
    public String loggedinUser;

    List<string> allUsers;

    string rootURL = "https://erinjktruesdell.com/";
    private void Awake()
    {
        updatePlayerList();
    }

    // Start is called before the first frame update
    void updatePlayerList()
    {
        StartCoroutine(getAllPlayers());
    }
    void HeardIt(TabButton t, EventArgs e)
    {
        updatePlayerList();
    }

    public IEnumerator getAllPlayers()
    {
        allUsers = new List<string>();
        // get data from server
        WWWForm form = new WWWForm();
        form.AddField("s", "s"); //dummy data


        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "/get-all-usernames.php", form))
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
                string[] userChunks = responseText.Split('|');
                foreach (string chunk in userChunks)
                {
                    if (chunk.Length > 0) // trim off that last empty bit
                    {
                        allUsers.Add(chunk);
                    }
                }



            }

            playerDropdown.ClearOptions();
            playerDropdown.AddOptions(allUsers);
            //break data into chunks
            selectUser(allUsers[0]);
            //iterate through chunks, downloading pfolie pics and creating prefabs


        }
    }

    public void getDropdownValue()
    {
        selectUser(playerDropdown.options[playerDropdown.value].text);
    }

    public void selectUser(string s)
    {
        loggedinUser = s;
    }
}



