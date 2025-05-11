using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndScreenHandler : MonoBehaviour
{
    public DayManager dayMan;
    public TMP_Text dayText;
    private void Start()
    {
        dayMan = FindObjectOfType<DayManager>();
    }
    private void OnEnable()
    {
        dayText.text = "End of Day: " + DayManager.currentDay;
    }
    public void GoToNextLevel()
    {
        dayMan.GoToNextDay();
    }
}
