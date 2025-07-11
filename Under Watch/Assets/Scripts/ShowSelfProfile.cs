using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSelfProfile : MonoBehaviour
{
    public GameObject zoomedImage;
    public void hideZoomedImage()
    {
        zoomedImage.SetActive(false);
    }

        public void showZoomedImage()
    {
        zoomedImage.SetActive(true);
    }
}
