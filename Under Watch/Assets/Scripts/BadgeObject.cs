using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BadgeObject : MonoBehaviour
{
    private TextMeshProUGUI bigNumText;
    public TextMeshProUGUI titleText;
    public List<GameObject> badgeIcons;
    //assign the bigNumText via getComponenting when we get the right badge icon

    public AchievementObject achObj;
    public void ConfigureAchievement(AchievementObject ach)
    {
        achObj = ach;
        titleText.text = achObj.title;
        DetermineBadging();
    }

    void DetermineBadging()
    {
        foreach (GameObject obj in badgeIcons)
        {
            obj.SetActive(false);
        }

        GameObject badge = badgeIcons[achObj.currentLevel];

        badge.SetActive(true);
        bigNumText = badge.GetComponentInChildren<TextMeshProUGUI>();
        bigNumText.text = achObj.currentLevel.ToString();
    }

    public void ClickOnAchievement()
    {
        PopUpObject popUp = PopUpObject.popUpInstance;
        popUp.ConfigurePopup(achObj.title, GetFormattedDescription(achObj));
    }

    public string GetFormattedDescription(AchievementObject _achObj)
    {
        int currLevel = _achObj.currentLevel;
        List<int> reqNums = _achObj.reqsPerLevel;

        string descStr = _achObj.description;
        //by default just return the description
        string returnString = descStr;

        //inserts the next req number wherever the {value} is found
        if (descStr.Contains("{value}"))
        {
            int replacer = reqNums[currLevel];
            descStr.Replace("{value}", replacer.ToString());

            if (descStr.Contains("{nth}"))
            {
                int replaceNum = reqNums[currLevel];
                string replacerStr = ReturnNthString(replaceNum);
                returnString = descStr.Replace("{nth}", replacerStr);
            }
        }
        return returnString;
    }

    public string ReturnNthString(int num)
    {
        //100% homegrown cage free code
        switch (num)
        {
            default:
                return "nth";
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            case 4 or 5 or 6 or 8:
                return "th";
            case int n when n == 7 || n >= 9:
                return "nth";
        }
    }
}
