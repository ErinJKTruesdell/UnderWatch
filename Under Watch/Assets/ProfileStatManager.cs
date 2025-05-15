using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class ProfileStatManager : MonoBehaviour
{
    public TMP_Text objectivesText;
    public TMP_Text postsText;
    public TMP_Text reactsText;

    public GameManager gm;

    private void OnEnable()
    {
        gm = FindObjectOfType<GameManager>();

        InitObjectivesCount();
        InitPostsAndReacts();
    }

    void InitPostsAndReacts()
    {
        StartCoroutine(GetProfileData());
    }

    void InitObjectivesCount()
    {
        objectivesText.text = DayManager.objCompleted.ToString();
    }

    IEnumerator GetProfileData()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", gm.scls.getUsername());
        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-profile-stats.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to load level: " + www.error + www.downloadHandler.text);
                yield break;
            }
            else
            {
                Debug.Log("response: " + www.downloadHandler.text);

                string[] partition = www.downloadHandler.text.Split("|");

                reactsText.text = partition[0];
                postsText.text = partition[1];
            }
        }
    }
}
