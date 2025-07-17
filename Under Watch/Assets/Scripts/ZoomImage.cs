using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZoomImage : MonoBehaviour
{
    public GameObject zoomedImage;
    public RawImage zoomedRawImage;
    public RawImage sourceTex;
    public void showZoomedImage()
    {
        zoomedRawImage.texture = sourceTex.texture;
        zoomedImage.SetActive(true);
    }
}
