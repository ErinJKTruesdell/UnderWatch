using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SeflieUI : MonoBehaviour
{
    /* string rootURL = "egs01.westphal.drexel.edu/";

     public TextMeshProUGUI playerUN;
     public TextMeshProUGUI targetUN;

     public RawImage playerPic;
     public RawImage targetPic;

     public SelfieCam selfieCam;
     public GameManager gm;
     public SC_LoginSystem scls;

     private void Start()
     {
         gm = FindObjectOfType<GameManager>();
         if (gm == null)
         {
             gm = new GameManager();
         }
         scls = gm.scls;

         playerUN.text = gm.scls.getUsername();


     }

     //is there a better way to get the target's username and their picture than two seperate PHP calls? Yes!
     public IEnumerator LocateTarget(string username)
     {
         WWWForm form = new WWWForm();
         form.AddField("username", username);

         string errorMessage = "";

         using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "get-target-location.php", form))
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
                 string[] dataChunks = responseText.Split('|');

                 targetUN.text = "Target: @" + dataChunks[1];
             }
             else
             {
                 errorMessage = responseText;
             }
             //}
         }


     }

     public IEnumerator GetTargetInfo(string username)
     {
       // get data from server
         WWWForm form = new WWWForm();
         form.AddField("s", "s"); //dummy data

         using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "get-usernames.php", form))
         {
             yield return www.SendWebRequest();

             if (www.result != UnityWebRequest.Result.Success)
             {
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
                         if (UNData[j] == )
                         {

                         }
                     }
                 }
             }
    */
}
