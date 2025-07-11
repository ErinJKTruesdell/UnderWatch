using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Linq;
public class ProfileAttributeManager : MonoBehaviour
{
    public List<(TMP_InputField response, string attName)> attributeResponses = new();

    public GameObject attributeObj;
    public Transform attParent;
    public GameObject privacyPolicy;
    public GameObject attPrefab;

    public TextMeshProUGUI responseText;

    private void Start()
    {
        foreach (TMP_InputField att in attParent.GetComponentsInChildren<TMP_InputField>())
        {
            attributeResponses.Add((att, ""));
        }
    }
    private void OnEnable()
    {
        responseText.text = "";
        attributeObj.SetActive(true);
        privacyPolicy.SetActive(false);

        StartCoroutine(GetAttributesData());
        SetupDoTweenPositions();
    }

    void SetupDoTweenPositions()
    {
        attributeObj.transform.DOLocalMoveX(1400, .5f).SetEase(Ease.OutQuad).From();
    }

    public void NextButton()
    {
        responseText.text = "";
        responseText.color = Color.white;

        if (CheckInput() == false)
        {
            return;
        }

        //moves and sets active privacy policy
        StartCoroutine(SendAttributesData());
    }

    bool CheckInput()
    {
        foreach (var attField in attributeResponses)
        {
            string response = attField.response.text;

            if (response.All(char.IsDigit))
                //skip if it's all a number
                continue;

            if (response == "")
            {
                responseText.text = "Missing one or more fields.";
                return false;
            }
            else if (response.Length < 3)
            {
                responseText.text = "Response in one or more fields is too short!";
                return false;
            }
        }
        return true;
    }
    IEnumerator GetAttributesData()
    {
        try
        {
            string un = GameManager.loggedInUser.un;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            SceneManager.LoadScene("LoginScene");
            yield break;
        }
        //gets 3 random attributes of all possible ones - NOT a user's attributes
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un); //dummy data
        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get_profile_attributes.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + www.error + www.downloadHandler.text);
                yield break;
            }
            else
            {
                Debug.Log("response: " + www.downloadHandler.text);
                HandleAttData(www.downloadHandler.text);
            }
        }
    }

    void HandleAttData(string response)
    {
        string[] attChunks = response.Split("|");

        foreach (string att in attChunks)
        {
            if (!string.IsNullOrEmpty(att))
            {
                string[] titleExPair = att.Split("@");

                GameObject attObj = Instantiate(attPrefab, attParent);
                attributeResponses.Add((attObj.GetComponent<TMP_InputField>(), titleExPair[0]));

                if (titleExPair.Length == 2)
                {
                    AttributeObject attContainer = attObj.GetComponent<AttributeObject>();
                    attContainer.title.text = titleExPair[0];
                    attContainer.example.text = titleExPair[1];
                }
                else
                    Debug.Log("Wrong format for attribute string");
            }
        }
    }

    IEnumerator SendAttributesData()
    {
        TrimResponses();

        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);
        form.AddField("workLoc", attributeResponses[0].response.text);

        form.AddField("att1", attributeResponses[1].response.text);
        form.AddField("att2", attributeResponses[2].response.text);
        form.AddField("att3", attributeResponses[3].response.text);

        form.AddField("att1Name", attributeResponses[1].attName);
        form.AddField("att2Name", attributeResponses[2].attName);
        form.AddField("att3Name", attributeResponses[3].attName);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "post-profile-attributes.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to send data!: " + www.error + www.downloadHandler.text);
                responseText.color = Color.red;
                responseText.text = www.error + www.downloadHandler.text;
                yield break;
            }
            else
            {
                Debug.Log("response: " + www.downloadHandler.text);

                //also moves the next button, it's inside attributeFields
                attributeObj.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad)
                        .OnComplete(() => attributeObj.SetActive(true));
                privacyPolicy.transform.DOLocalMoveX(1400, .5f).SetEase(Ease.OutQuad).From()
                    .OnComplete(() => privacyPolicy.SetActive(true));
            }
        }
    }

    void TrimResponses()
    {
        foreach (var attField in attributeResponses)
        {
            attField.response.text.Trim().ToLower();
            attField.attName.Trim().ToLower();
        }
    }
    
}
