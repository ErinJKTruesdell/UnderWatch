using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrivacyPolicyManager : MonoBehaviour
{
    public TMP_Text detailsTitleText;
    public TMP_Text detailsDescText;
    public GameManager gm;

    public Transform containersParent;
    public PrivacyPolicyData[] dataContainers;

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
        gm.ProgressToScene("SocialFeed");
        RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.privacyPolicy);
        RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.profilePic);
    }


}
