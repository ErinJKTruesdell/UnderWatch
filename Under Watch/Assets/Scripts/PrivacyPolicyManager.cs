using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PrivacyPolicyManager : MonoBehaviour
{
    public TMP_Text detailsTitleText;
    public TMP_Text detailsDescText;
    public GameManager gm;

    public Transform containersParent;
    public PrivacyPolicyData[] dataContainers;

    bool tweenFinished = false;
    int policiesShown = 0;
    private void Awake()
    {
        dataContainers = containersParent.GetComponentsInChildren<PrivacyPolicyData>();
    }
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        ActivatePrivacyPolicy();
    }

    private void ActivatePrivacyPolicy()
    {
        foreach (PrivacyPolicyData data in dataContainers)
        {
            if (data.day == DayManager.currentDay)
            {
                detailsTitleText.text = data.title;
                detailsDescText.text = data.desc;
                break;
            }
        }
    }
    public void AgreeToPolicy()
    {
        StartCoroutine(ActivateAccount());
        transform.DOLocalMoveY(1400f, .5f).SetEase(Ease.OutQuad)
            .OnComplete(() => tweenFinished = true);
    }

    IEnumerator ActivateAccount()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);
        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "toggle-user-enabled.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + www.error + www.downloadHandler.text);
                yield break;
            }
            else
            {
                Debug.Log("response: " + www.downloadHandler.text);
            }
        }
        yield return new WaitUntil(() => tweenFinished);
        gm.ProgressToScene("SocialFeed");
    }
}
