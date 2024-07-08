using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomBarButton : MonoBehaviour
{

    public GameObject toTurnOn;
    public GameObject[] toTurnOff;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void turnObjectOn()
    {
        toTurnOn.SetActive(true);
        foreach(GameObject obj in toTurnOff)
        {
            obj.SetActive(false);
        }
    }
}
