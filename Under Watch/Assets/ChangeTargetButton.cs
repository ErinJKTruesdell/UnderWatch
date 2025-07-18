using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTargetButton : MonoBehaviour
{
    public GameObject confirmPopUp;

    public void ClickChangeTarget()
    {
        confirmPopUp.SetActive(true);
    }
    public void ConfirmChangeTarget()
    {

    }

    public void CLoseConfirm()
    {
        confirmPopUp.SetActive(false);
    }

}
