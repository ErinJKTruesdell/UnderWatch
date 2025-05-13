using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndScreenHandler : MonoBehaviour
{
    public DayManager dayMan;
    public GameManager gm;
    public TMP_Text dayText;

    public GameObject privacyPolicy;
    private void Awake()
    {
        privacyPolicy.SetActive(false);
    }
    private void Start()
    {
        dayMan = FindObjectOfType<DayManager>();
        gm = FindObjectOfType<GameManager>();
    }
    private void OnEnable()
    {
        dayText.text = "End of Day: " + DayManager.currentDay;
    }
    public void GoToNextLevel()
    {
        dayMan.ProgressDay();

        if (!DayManager.DoesDayContainObjective(DayManager.ObjTypes.privacyPolicy))
        {
            gm.ProgressToScene("SocialFeed");
        }
        else
        {
            privacyPolicy.SetActive(true);
        }
    }
}
