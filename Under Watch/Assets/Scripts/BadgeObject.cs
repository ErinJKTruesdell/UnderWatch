using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BadgeObject : MonoBehaviour
{
    private TextMeshProUGUI bigNumText;
    public TextMeshProUGUI titleText;
    public List<GameObject> badgeIcons;
    public Button popUpButton;
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

        int progressNum = Mathf.Clamp(achObj.progress, 0, 99);
        bigNumText.text = progressNum.ToString();

        if (achObj.currentLevel > 0)
        {
            popUpButton.interactable = true;
            popUpButton.image = badge.GetComponent<Image>();
        }
        else
            popUpButton.interactable = false;
    }

    public void ClickOnAchievement()
    {
        PopUpObject popUp = PopUpObject.popUpInstance;
        popUp.ConfigurePopup(achObj.title, AchievementManager.GetFormattedDescription(achObj));
    }
}
