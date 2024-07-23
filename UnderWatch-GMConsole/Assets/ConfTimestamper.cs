using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfTimestamper : MonoBehaviour
{

    public TMP_Dropdown monthDrop;
    public TMP_Dropdown dayDrop;
    public TMP_Dropdown hourDrop;

    Dictionary<int, int> month_days = new Dictionary<int, int>()
    {
        {0,31 }, //july
        {1,31 }, //aug
        {2,30 }, //sept
        {3,31 }, //oct
    };

    // Start is called before the first frame update
    void Start()
    {
        onMonthUpdate();

        hourDrop.ClearOptions();
        List<string> newOptions = new List<string>();
        for (int i = 0; i < 24; i++)
        {
            newOptions.Add((i).ToString());
        }
        hourDrop.AddOptions(newOptions);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onMonthUpdate()
    {
        dayDrop.ClearOptions();
        List<string> newOptions = new List<string>();
        for (int i = 0; i < month_days[monthDrop.value]; i++)
        {
            newOptions.Add((i + 1).ToString());
        }
        dayDrop.AddOptions(newOptions);
    }


    public Tuple<int, int,int> selectedTimestamp()
    {
        return new Tuple<int, int,int>(monthDrop.value + 7, dayDrop.value + 1,hourDrop.value); //account for offset since we start in july, and off-by-one on days due to zero-based indexing
    }

}
