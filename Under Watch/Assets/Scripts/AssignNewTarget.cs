using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignNewTarget : MonoBehaviour
{
    public GameObject popupPanel;

    public GameManager gm;
    public SC_LoginSystem scls;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            gm = new GameManager();
        }
        scls = gm.scls;
    }
    public void ClickedButton()
    {
        popupPanel.SetActive(true);
    }

    public void ClickedClosePopup()
    {
        popupPanel.SetActive(false);
    }

    public void ChangeTarget()
    {
        scls.doTargetAssignment(scls.getUsername(), -25);
        ClickedClosePopup();
    }

}
