using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSelfProfile : MonoBehaviour
{
    public GameObject zoomedImage;

    public ProfileDatabase pd;
    public PlayerViewTab pvt;

    // Start is called before the first frame update
    private void Awake()
    {
        pvt.dropdownChanged += new PlayerViewTab.DropdownChangeHandler(doRefresh);
    }

    public void onActivate()
    {
        StartCoroutine(delayLoad());
    }

    public void hideZoomedImage()
    {
        zoomedImage.SetActive(false);
    }

    public void doRefresh(EventArgs e)
    {

        onActivate();
    }

    public IEnumerator delayLoad()
    {
        Debug.Log("Command Received");
        while (!pd.gameObject.activeSelf)
        {
            yield return null;
        }
        Debug.Log("GO TIME");
        pd.fillCanvas(pvt.loggedinUser);
    }

}
