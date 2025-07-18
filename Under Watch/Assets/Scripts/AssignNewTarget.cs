using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        StartCoroutine(ChangeTargetRoutine());
    }

    IEnumerator ChangeTargetRoutine()
    {
        string username = GameManager.loggedInUser.un;
        yield return StartCoroutine(scls.doTargetAssignment(username));
        PointsManager.AddPoints(username, PointsManager.negChangeTargetPoints, PointsManager.Source.NegChangeTaret);

        ClickedClosePopup();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
