using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRidOfPrivacyPolicy : MonoBehaviour
{
    public GameObject popUp;
    public void ClosePolicy()
    {
        popUp.SetActive(false);
    }

}
