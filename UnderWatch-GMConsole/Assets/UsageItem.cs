using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UsageItem : MonoBehaviour
{
    public TMP_Text uid;
    public TMP_Text username;
    public TMP_Text social;
    public TMP_Text total;

    // Start is called before the first frame update
    public void setStats(string userid, string un, float socialtime, float totaltime)
    {
        uid.text = userid;
        username.text = "@" + un;
        social.text = "social: " + socialtime.ToString("n2") + " minutes";
        total.text = "total: " + totaltime.ToString("n2") + " minutes";
    }
}
