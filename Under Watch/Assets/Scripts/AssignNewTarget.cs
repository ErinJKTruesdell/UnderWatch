using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignNewTarget : MonoBehaviour
{
    public GameObject popupPanel;

    public GameManager gm;
    public SC_LoginSystem scls;
    public GetTargetLocation getTargetLoc;
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
        StartCoroutine(ChangeTargetRoutine());
    }

    IEnumerator ChangeTargetRoutine()
    {
        string username = scls.getUsername();
        yield return StartCoroutine(scls.doTargetAssignment(username, -25));
        StartCoroutine(getTargetLoc.LocateTarget(username));

        ClickedClosePopup();
    }

}
