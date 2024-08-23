using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePicHandling : MonoBehaviour
{
    public ProfileDatabase pfd;

    public RawImage zoomedProfImage;
    public GameObject zoomeProfImageObj;

    private void Start()
    {
        zoomeProfImageObj.SetActive(false);
    }
    public void ClickOnZoomedProfile()
    {
        //Im not sure why its done this way elsewhere, but lets stay consistent
        zoomeProfImageObj.SetActive(true);
        zoomedProfImage.texture = pfd.profileImage.texture;
    }
    public void hideZoomedProfImage()
    {
        zoomeProfImageObj.SetActive(false);
    }
}
