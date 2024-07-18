using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PasswordQueryManager : MonoBehaviour
{

    public TMP_InputField emailField;
    public TMP_InputField codeField;
    public TMP_InputField pwField;
    public TMP_Text errorText;
    GameManager gm;

    public GameObject requestButton;
    public GameObject resetButtonItems;

    string currentEmail = "";
    string newPw = "";


    // Start is called before the first frame update
    void Start()
    {

        gm = GameObject.FindObjectOfType<GameManager>();
        if(gm == null)
        {
            gm = new GameManager();
        }

        gm.scls.pqm = this;
    }

    public void receiveQueryResponse(string response)
    {
        if (response.StartsWith("Success"))
        {
            requestButton.SetActive(false);
            resetButtonItems.SetActive(true);
            errorText.text = "";
            emailField.interactable = false;
        }
        else
        {
            errorText.text = response;
        }
    }

    public void receiveResetResponse(string response)
    {
        if (response.StartsWith("Success"))
        {
            //log the user in and move on
            StartCoroutine(gm.scls.LoginEnumerator(currentEmail, newPw));
        }
        else
        {
            errorText.text = response;
        }
    }

    public void requestReset()
    {
        currentEmail = emailField.text;
        StartCoroutine(gm.scls.sendResetRequest(currentEmail));
    }

    public void sendResetData()
    {
        newPw = pwField.text;
        gm.scls.SetLoginPrefs(emailField.text, pwField.text);
        StartCoroutine(gm.scls.sendResetUpdatePassword(currentEmail, codeField.text.Trim(), newPw));
    }



}
