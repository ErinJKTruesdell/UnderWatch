using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour
{

    public event ActivatedHandler Activated;
    public EventArgs e = null;
    public delegate void ActivatedHandler(TabButton t, EventArgs e);

    public GameObject tabObject;
    public string tabName;
    public Image tabImage;

    public void sendActiveEvent()
    {
        StartCoroutine(eventWait());
    }

    public IEnumerator eventWait()
    {
        yield return new WaitForSeconds(0.5f);
        if (Activated != null)
        {
            Activated(this, e);
        }
    }
}
