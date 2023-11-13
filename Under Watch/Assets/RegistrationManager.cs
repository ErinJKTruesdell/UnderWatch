using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegistrationManager : MonoBehaviour
{


    public Text email;
    public Text password;
    public Text username;

    public Text errorText;

    SC_LoginSystem loginSystem;


    // Start is called before the first frame update
    void Start()
    {
        loginSystem = GameObject.FindObjectOfType<SC_LoginSystem>();

        if(loginSystem == null)
        {
            loginSystem = new SC_LoginSystem();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
