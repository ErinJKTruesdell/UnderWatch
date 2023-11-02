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

    public MeshRenderer rearMesh;
    public MeshRenderer frontMesh;

    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    Vector3 currentLocalEurlerAngles = Vector3.zero;
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
        rearMesh.material.SetTexture("_MainTex", webcam0);

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



        Vector3[] corners = new Vector3[4];
        rear.rectTransform.GetWorldCorners(corners);
        Vector3 topLeft = corners[0];

        var width = (int)(corners[3].x - corners[0].x); //.rect.width;
        var height = (int)(corners[1].y - corners[0].y);
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Rescale the size appropriately based on the current Canvas scale
        Vector2 scaledSize = new Vector2(width, height);

        

        tex.ReadPixels(new Rect(topLeft, scaledSize), 0, 0);
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
            rearMesh.material.SetTexture("_MainTex", snap);
            //byte[] bytes = snap.EncodeToPNG();
            frontMesh.gameObject.SetActive(true);
            webcam0.Stop();
            webcam1 = new WebCamTexture(devices[1].name);
            webcam1.Play();
            frontMesh.material.SetTexture("_MainTex",webcam1);
            rearTaken = true;

        }
        else
        {
            Texture2D snap = new Texture2D(webcam1.width, webcam1.height);
            snap.SetPixels(webcam1.GetPixels());
            snap.Apply();
            frontMesh.material.SetTexture("_MainTex", snap);
            webcam1.Stop();
            StartCoroutine(takeSnap());
        }
    }
}
