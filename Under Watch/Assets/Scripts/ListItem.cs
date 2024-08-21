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

        //how expensive is doing this rather than just putting the script in the scene?
        scls = new SC_LoginSystem();

        unText.text = sfd.currentProfileUsername;
        locText.text = sfd.Lat;


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
