using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignNewTarget : MonoBehaviour
{
    public GameObject popupPanel;
    public void ClickedButton()
    {
        popupPanel.SetActive(true);
    }

    public void ClickedClosePopup()
    {
        popupPanel.SetActive(false);
    }
}
