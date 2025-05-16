using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndScreenHandler : MonoBehaviour
{
    public DayManager dayMan;
    public GameManager gm;
    public TMP_Text dayText;
    public TMP_Text dayDescText;
    public TMP_Text dayCounter;
    public TMP_Text objCounter;

    [TextArea] public string FinalDayDesc;

    public GameObject privacyPolicy;
    public GameObject loginButton;

    int currentDay;
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
        currentDay = DayManager.currentDay;
        dayText.text = "End of Day: " + currentDay;
        dayCounter.text = currentDay + " / " + DayManager.maxDays;
        objCounter.text = DayManager.objCompleted + " / " + "41 Objectives" ;
        if (currentDay >= DayManager.maxDays)
        {
            EndGame();
        }
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

    public void EndGame()
    {
        dayDescText.text = FinalDayDesc;
        loginButton.SetActive(false);
        PlayerPrefs.DeleteAll();
    }
}
