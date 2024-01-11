using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelfieCam : MonoBehaviour
{

    public RawImage rear;
    WebCamDevice[] devices;

    WebCamTexture webcam;


    public MeshRenderer camMesh;
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public GameManager gm;
    public SC_LoginSystem scls;

    public GameObject testBox;

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
        webcam = new WebCamTexture(devices[1].name);


        webcam.Play();
        camMesh.material.SetTexture("_MainTex", webcam);

        gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            gm = new GameManager();
        }
        scls = gm.scls;

        scls.Target += new SC_LoginSystem.TargetHandler(showNewTarget);
    }

    public void showNewTarget(string s, EventArgs e)
    {
        if (s.StartsWith("Success"))
        {
            testBox.SetActive(true);
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
        string filename = gm.scls.getUsername() + "-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".png";
        string path = Application.persistentDataPath + filename;
        Debug.Log("--------------------------------------SAVING TO PATH--------------------------------------");
        Debug.Log(path);
        Debug.Log("------------------------------------------------------------------------------------------");
        System.IO.File.WriteAllBytes(path, bytes);

        bool isTarget = doFaceRecognition(path);

        if (isTarget)
        {
            //show success popup

            //assign points
            StartCoroutine(scls.doTargetAssignment(scls.getUsername(), 1));

        }
        else
        {
            //show failure popup
        }
    }

    public void capturePhoto()
    {
        Texture2D snap = new Texture2D(webcam.width, webcam.height);
        snap.SetPixels(webcam.GetPixels());
        snap.Apply();
        camMesh.material.SetTexture("_MainTex", snap);
        //byte[] bytes = snap.EncodeToPNG();
        webcam.Stop();

        StartCoroutine(takeSnap());

    }

    public bool doFaceRecognition(string path)
    {
        //TODO https api call goes here!
        return true;
    }
}
