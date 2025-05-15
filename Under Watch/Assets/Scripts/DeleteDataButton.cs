using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteDataButton : MonoBehaviour
{
    GameManager gm;
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    public void LogOutButton()
    {
        gm.LogOut();
    }
}
