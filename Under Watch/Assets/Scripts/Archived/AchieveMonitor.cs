using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchieveMonitor : MonoBehaviour
{
    public static AchieveMonitor achmonInstance;
    public int adClicks;
    public int favorites;
    public int minutes;

    public AchievementsManager achMan;
    public void addAdClick(int num = 1)
    {
        adClicks += num;
        PlayerPrefs.SetInt("adClicks", adClicks);
        PlayerPrefs.Save();

        achMan.UpdateAchievement(title: "Super Supporter", tierThresholds: achMan.superSupporterTiers, counterVar: adClicks, achIndex: 0, descStart: "click on", descEnd: "ads", winDesc: "you've clicked all the ads!");

    }

    public void addFavorite()
    {
        favorites++;
        PlayerPrefs.SetInt("favorites", favorites);
        PlayerPrefs.Save();

        achMan.UpdateAchievement(title: "Social Butterfly", tierThresholds: achMan.socialButterflyTiers, counterVar: favorites, achIndex: 1, descStart: "favorite", descEnd: "player profiles", winDesc: "you're a real buttefly!");

    }

    public void addMinutes(double mins)
    {
        minutes += Convert.ToInt32(mins);
        PlayerPrefs.SetInt("minutes", minutes);
        PlayerPrefs.Save();

        achMan.UpdateAchievement(title: "I Spy Master", tierThresholds: achMan.iSpyMasterTiers, counterVar: minutes, achIndex: 1, descStart: "interact with the feed for", descEnd: "minutes", winDesc: "you're practically [INSERT COPYRIGHT FREE SPY NAME]!");

    }

    private void Awake()
    {
        if (achmonInstance == null)
        {
            achmonInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
