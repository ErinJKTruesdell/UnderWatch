using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleTargetScreen : MonoBehaviour
{
    public DisplayTargetProfile targetProf;

    private void Start()
    {
        if (targetProf == null)
            targetProf = FindObjectOfType<DisplayTargetProfile>();
        targetProf.ConfigureUser(GameManager.currTarget);
    }
}
