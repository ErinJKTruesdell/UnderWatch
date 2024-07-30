using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem : MonoBehaviour
{
    // Start is called before the first frame update
    public RawImage photoImg;
    public RawImage profImage;
    public TMP_Text unText;
    public TMP_Text postIDText;

    public SC_LoginSystem scls;
    public SocialFeedDatabase sfd;

    void Start()
    {
        if (sfd == null)
        {
            sfd = FindObjectOfType<SocialFeedDatabase>();
        }

        //how expensive is doing this rather than just putting the script in the scene?
        scls = new SC_LoginSystem();

        unText.text = sfd.currentProfileUsername;
    }
}
