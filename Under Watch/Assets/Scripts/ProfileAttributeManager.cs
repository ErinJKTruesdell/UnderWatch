using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static OnlineMapsGPXObject;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class ProfileAttributeManager : MonoBehaviour
{
    public List<TMP_InputField> attributeResponses = new();

    public GameObject attributeObj;
    public Transform attParent;
    public GameObject privacyPolicy;
    public GameObject attPrefab;

    public TextMeshProUGUI responseText;

    private void Start()
    {
        foreach (TMP_InputField att in attParent.GetComponentsInChildren<TMP_InputField>())
        {
            attributeResponses.Add(att);
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
        string responseValid = "";
        responseText.text = "";
        responseText.color = Color.white;

        Debug.Log("????");

        foreach (TMP_InputField attField in attributeResponses)
        {
            if (attField.text == "")
            {
                responseValid = "Missing one or more fields.";
                break;
            }
            else if (attField.text.Length < 3)
            {
                responseValid = "Response in one or more fields is too short!";
                break;
            }
        }
        if (responseValid != "")
        {
            responseText.text = responseValid;
            responseValid = "";
            return;
        }

        //moves and sets active privacy policy
        StartCoroutine(SendAttributesData());
    }

    IEnumerator GetAttributesData()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);
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
        //having a wierd issue with this not working bc one of the chukns is empty

        foreach (string att in attChunks)
        {
            if (!string.IsNullOrEmpty(att))
            {
                GameObject attObj = Instantiate(attPrefab, attParent);
                attributeResponses.Add(attObj.GetComponent<TMP_InputField>());

                string[] titleExPair = att.Split("@");
                if (titleExPair.Length == 2)
                {
                    AttributeObject attContainer = attObj.GetComponent<AttributeObject>();
                    attContainer.title.text = titleExPair[0];
                    attContainer.placeHolderText.text = titleExPair[1];
                }
                else
                    Debug.Log("Wrong format for attribute string");
            }
        }
    }

    IEnumerator SendAttributesData()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);
        form.AddField("workLoc", attributeResponses[0].text);

        form.AddField("att1", attributeResponses[1].text);
        form.AddField("att2", attributeResponses[2].text);
        form.AddField("att3", attributeResponses[3].text);

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
}
