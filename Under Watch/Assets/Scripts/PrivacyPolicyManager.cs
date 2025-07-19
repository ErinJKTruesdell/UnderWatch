using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;

public class PrivacyPolicyManager : MonoBehaviour
{
    public TMP_Text detailsTitleText;
    public TMP_Text detailsDescText;
    public GameManager gm;

    public Transform containersParent;
    public GameObject PrivacyPolicyPopup;
    public PrivacyPolicyData[] dataContainers;

    bool tweenFinished = false;
    int policiesShown = 0;
    private void Awake()
    {
        dataContainers = containersParent.GetComponentsInChildren<PrivacyPolicyData>();

        if (SceneManager.GetActiveScene().name == "ProfileSetup")
        {
            ActivatePrivacyPolicy();
        }
    }
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "ProfileSetup")
            DeterminePrivacyPolicy();
    }

    void DeterminePrivacyPolicy()
    {
        DateTime startDate = new DateTime(2025, 7, 18);
        int day = (DateTime.UtcNow.Date - startDate).Days;
        Debug.Log("day: " + day);

        foreach (PrivacyPolicyData data in dataContainers)
        {
            if (data.day == day && PlayerPrefs.GetInt("seenDay") != day)
            {
                PlayerPrefs.SetInt("seenDay", day);
                PlayerPrefs.Save();

                PrivacyPolicyPopup.SetActive(true);

                ActivatePrivacyPolicy();
                return;
            }
        }
    }

    private void ActivatePrivacyPolicy()
    {
        Debug.Log("sdfjkh");
        DateTime startDate = new DateTime(2025, 7, 18);
        int day = (DateTime.UtcNow.Date - startDate).Days;

        foreach (PrivacyPolicyData data in dataContainers)
        {
            if (data.day == day)
            {
                PrivacyPolicyPopup.SetActive(true);
                detailsTitleText.text = data.title;
                detailsDescText.text = data.desc;
                break;
            }
        }
    }
    public void AgreeToPolicy()
    {
        if (SceneManager.GetActiveScene().name == "ProfileSetup")
        {
            StartCoroutine(ActivateAccount());

            PrivacyPolicyPopup.transform.DOLocalMoveY(1400f, .5f).SetEase(Ease.OutQuad)
    .OnComplete(() => tweenFinished = true);
        }
        PrivacyPolicyPopup.transform.DOLocalMoveY(1400f, .5f).SetEase(Ease.OutQuad)
            .OnComplete(() => PrivacyPolicyPopup.SetActive(false));

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
