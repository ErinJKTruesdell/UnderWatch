using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSelfProfile : MonoBehaviour
{
    public GameObject zoomedImage;

    public ProfileDatabase pd;
    SC_LoginSystem loginSystem;

    // Start is called before the first frame update
    private void Awake()
    {
        loginSystem = GameObject.FindObjectOfType<SC_LoginSystem>();
        if (loginSystem == null)
        {
            loginSystem = new SC_LoginSystem();
        }
        // pd.fillCanvas(loginSystem.getUsername());
        pd.fillCanvas("EJKTruesdell", "Texas Red");
    }

    public void hideZoomedImage()
    {
        zoomedImage.SetActive(false);
    }
}
