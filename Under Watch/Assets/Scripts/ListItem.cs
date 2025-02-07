using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ListItem : MonoBehaviour
{
    // Start is called before the first frame update
    public RawImage photoImg;
    public RawImage profImage;
    public TMP_Text unText;
    public TMP_Text locText;
    public TMP_Text postIDText;

    public SC_LoginSystem scls;
    public SocialFeedDatabase sfd;

    public GameObject[] unsponsoredObjs;
    public GameObject[] sponsoredObjs;

    void Start()
    {
        if (sfd == null)
        {
            sfd = FindObjectOfType<SocialFeedDatabase>();
        }

        scls = new SC_LoginSystem();

        if (sfd.isAd)
        {
            unText.text = "Sponsored";

            foreach (GameObject obj in unsponsoredObjs)
            {
                obj.SetActive(false);
            }

            foreach (GameObject obj in sponsoredObjs)
            {
                obj.SetActive(true);
            }
        }
    }

    public void ClickOnProfile()
    {
        ShowClickedProfile.userName = unText.text;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }




}
