using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CameraDisplay : MonoBehaviour
{
    public RawImage front;
    public RawImage rear;
    WebCamDevice[] devices;

    bool rearTaken = false;
    int frontFrames = 5;
    int frontCount = 0;
    WebCamTexture webcam1;
    WebCamTexture webcam0;
    // Start is called before the first frame update
    void Start()
    {

        devices = WebCamTexture.devices;
        WebCamDevice frontCamera;
        for (int i = 1; i < devices.Length; i++)
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

    public void capturePhoto()
    {
        if (!rearTaken)
        {
            Texture2D snap = new Texture2D(webcam0.width, webcam0.height);
            snap.SetPixels(webcam0.GetPixels());
            snap.Apply();
            rear.texture = snap;
            //byte[] bytes = snap.EncodeToPNG();
            front.gameObject.SetActive(true);
            webcam0.Stop();
            webcam1 = new WebCamTexture(devices[1].name);
            webcam1.Play();
            front.texture = webcam1;
            rearTaken = true;

            // string filename = fileName(Convert.ToInt32(snap.width), Convert.ToInt32(snap.height));
            // %path = Application.persistentDataPath + "/Snapshots/" + filename;
            // %System.IO.File.WriteAllBytes(path, bytes);
        }
        else
        {
            Texture2D snap = new Texture2D(webcam1.width, webcam1.height);
            snap.SetPixels(webcam1.GetPixels());
            snap.Apply();
            front.texture = snap;
            webcam1.Stop();
            //byte[] bytes = snap.EncodeToPNG();
        }
    }
}
