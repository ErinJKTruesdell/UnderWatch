using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomBarButton : MonoBehaviour
{

    public GameObject toTurnOn;
    public GameObject[] toTurnOff;

    public bool isProfile = false;

    public Image ri;
    public Color selectedColor;

    public Image[] toReset;

    public Image thisIcon;
    public Image[] iconsToReset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void turnObjectOn()
    {
        toTurnOn.SetActive(true);
        ri.color = selectedColor;
        thisIcon.color = Color.white;
        if(isProfile)
        {
            toTurnOn.GetComponentInChildren<ShowSelfProfile>().onActivate();
        }
        foreach(Image other in toReset)
        {
            other.color = Color.white;
        }
        foreach(GameObject obj in toTurnOff)
        {
            obj.SetActive(false);
        }
        foreach(Image i in iconsToReset)
        {
            i.color = Color.black;
        }
    }
}
