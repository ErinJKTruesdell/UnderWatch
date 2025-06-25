using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Net;
using DG.Tweening.Plugins.Core.PathCore;

public class SelfieCam : MonoBehaviour
{
    public RawImage rear;
    WebCamDevice[] devices;

    WebCamTexture webcam;

    public MeshRenderer camMesh;
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public GameObject overlay;

    public TextMeshProUGUI responseText;

    Vector3 currentLocalEurlerAngles = Vector3.zero;

    public SelfieUploader selfieUploader;
    public RegistrationManager regManager;

    void OnEnable()
    {
        InitWebcam();
    }

    public void InitWebcam()
    {
        overlay.SetActive(true);
        devices = WebCamTexture.devices;
        if (devices.Length > 1)
        {
            foreach(var device in devices)
            {
                if (device.isFrontFacing)
                {
                    webcam = new WebCamTexture(device.name);

                    if (webcam != null)
                    {
                        break;
                    }
                }
            }
            if (Application.isEditor)
            {
                webcam = new WebCamTexture(devices[1].name);
                Debug.Log("starting webcam: " + devices[1].name);
            }

            /*if (device[1].name != " ")
            {
                if (UnityEngine.Application.platform == RuntimePlatform.Android)
                    webcam = new WebCamTexture(devices[1].name);
                else
                    webcam = new WebCamTexture(devices[1].name);
                Debug.Log("cam: " + devices[1].name);
            }*/

            webcam.Play();
            camMesh.material.SetTexture("_MainTex", webcam);
        }
        else
        {
            responseText.color = Color.red;
            responseText.text = "No camera detected";
        }
    }

    public IEnumerator takeSnap()
    {
        yield return frameEnd;
        byte[] bytes = EncodePhoto().EncodeToPNG();
        string loggedInUser = SC_LoginSystem.getUsername();

        string filename = loggedInUser + "-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".png";
        string path = Application.persistentDataPath + filename;
        System.IO.File.WriteAllBytes(path, bytes);

        Debug.Log("File Upload Coroutine");
        if (selfieUploader != null)
        {
            StartCoroutine(selfieUploader.SelfieUpload(path));
        }
        if (regManager != null)
        {
            //do the registration camera stuff
        }
    }
    private Texture2D EncodePhoto()
    {
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
        return tex;
    }
    public void capturePhoto()
    {
        if (devices.Length > 1)
        {
            if (webcam == null || !webcam.isPlaying)
                InitWebcam();

            overlay.SetActive(false);

            Texture2D snap = new Texture2D(webcam.width, webcam.height);
            snap.SetPixels(webcam.GetPixels());
            snap.Apply();
            camMesh.material.SetTexture("_MainTex", snap);
            //byte[] bytes = snap.EncodeToPNG();
            webcam.Stop();
            StartCoroutine(takeSnap());
        }
        else
        {
            responseText.color = Color.red;
            responseText.text = "No camera detected";
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && (webcam == null || !webcam.isPlaying))
        {
            Debug.Log("App regained focus. Restarting webcam...");
            InitWebcam();
        }
    }

}
