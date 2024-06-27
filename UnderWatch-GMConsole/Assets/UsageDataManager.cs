using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsageDataManager : MonoBehaviour
{
    public TabButton usageTab;
    public LeaderboardDatabase database;
    public UsageDataGetter udg;

    // Start is called before the first frame update
    void Awake()
    {
        if (usageTab != null)
        {
            usageTab.Activated += new TabButton.ActivatedHandler(HeardIt);
        }
    }

    void HeardIt(TabButton t, EventArgs e)
    {
        RefreshUsageData();
    }

    public void RefreshUsageData()
    {
        database.doLeaderboard();
        udg.doUsageData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}
