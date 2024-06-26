using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsageDataManager : MonoBehaviour
{
    public TabButton usageTab;
    public LeaderboardDatabase database;

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

    void RefreshUsageData()
    {
        database.doLeaderboard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}
