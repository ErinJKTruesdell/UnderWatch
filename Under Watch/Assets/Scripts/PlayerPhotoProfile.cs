using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPhotoProfile : MonoBehaviour
{
    public RawImage zoomedImage;
    public RawImage thisImage;

    public GameObject zoomedImageObj;

    public void setZoomedImage()
    {
        zoomedImage.texture = thisImage.texture;
        zoomedImageObj.SetActive(true);
    }


}
