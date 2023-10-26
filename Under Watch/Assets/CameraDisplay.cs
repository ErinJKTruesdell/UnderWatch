using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

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

    public UploadImage uploader;

    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
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

        uploader = GameObject.FindObjectOfType<UploadImage>();
        if(uploader == null )
        {
            uploader = new UploadImage();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator takeSnap()
    {
        yield return frameEnd;


        var width = (int)rear.rectTransform.rect.width;
        var height = (int)rear.rectTransform.rect.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(rear.rectTransform.rect.position.x, rear.rectTransform.rect.position.y, rear.rectTransform.rect.width, rear.rectTransform.rect.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        string filename = uploader.loginSystem.getUsername() + "-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".png";
        string path = Application.persistentDataPath + filename;
        Debug.Log("--------------------------------------SAVING TO PATH--------------------------------------");
        Debug.Log(path);
        Debug.Log("------------------------------------------------------------------------------------------");
        System.IO.File.WriteAllBytes(path, bytes);
        uploader.uploadIt(path);
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

        }
        else
        {
            Texture2D snap = new Texture2D(webcam1.width, webcam1.height);
            snap.SetPixels(webcam1.GetPixels());
            snap.Apply();
            front.texture = snap;
            webcam1.Stop();
            StartCoroutine(takeSnap());
        }
    }
}
