using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraDisplay : MonoBehaviour
{
    public RawImage front;
    public RawImage rear;
    WebCamDevice[] devices;

    bool isFront = true;
    int frontFrames = 5;
    int frontCount = 0;
    WebCamTexture webcam1;
    WebCamTexture webcam0;
    // Start is called before the first frame update
    void Start()
    {

        devices = WebCamTexture.devices;
        WebCamDevice frontCamera;
        for(int i = 1; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                frontCamera = devices[i];
                break;
            }
        }
        webcam0 = new WebCamTexture(devices[0].name);
        webcam0.Play();
        rear.texture = webcam0;


    }

    // Update is called once per frame
    void Update()
    {

    }
}
