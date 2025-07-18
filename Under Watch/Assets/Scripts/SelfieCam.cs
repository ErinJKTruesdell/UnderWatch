using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Net;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine.Android;

public class SelfieCam : MonoBehaviour
{
    public RawImage camView;
    WebCamDevice[] devices; 
    public WebCamTexture webcam;

    public GameObject overlay;
    public GameObject button;

    public Transform camTransform;

    public SelfieUploader selfieUploader;
    public RegistrationManager regManager;

    public Texture2D tex;
    private void Awake()
    {
        if (selfieUploader == null)
        {
            selfieUploader = FindObjectOfType<SelfieUploader>();
        }

        if (regManager == null)
        {
            regManager = FindObjectOfType<RegistrationManager>();
        }
    }
    void OnEnable()
    {
        InitWebcam();
    }
    public void InitWebcam()
    {
        CleanupWebcam();
        overlay.SetActive(true);
        button.SetActive(true);

        // Request permissions first, then initialize camera
        StartCoroutine(RequestCamPerms(() => {
            FindCameras();
        }));
    }
    IEnumerator RequestCamPerms(Action onAuthorized)
    {
#if UNITY_IOS || UNITY_WEBGL
        StartCoroutine(AskForPermissionIfRequired(UserAuthorization.WebCam, () => {
            onAuthorized?.Invoke();
        }));
        return null;
#elif UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            AskCameraPermission();

            // Wait for user response
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Camera));

            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                onAuthorized?.Invoke(); 
            }
            else
            {
                ErrorEventHandler.InvokeError("Permission Issue:", "Camera permissions not authorized! Please fix this in your device settings", Color.red);
            }
        }
#endif
        onAuthorized?.Invoke();
    }

    void FindCameras(bool frontFaceCare = true)
    {
        devices = WebCamTexture.devices;
        //can't be a foreach for some constructor related reason
        if (devices.Length > 0)
        {
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log("length: " + devices.Length + "name: " + devices[i].name + devices[i].isFrontFacing);

                if (devices[i].isFrontFacing || frontFaceCare == false)
                {
                    GetHighestAvailableResolution(devices[i], out int bestW, out int bestH);

                    webcam = new WebCamTexture(devices[i].name, bestW, bestH);

                    StartCoroutine(VerifyWebcamStarted());
                    break;
                }
            }

            // If no front-facing camera found and we were looking for one
            if (webcam == null && frontFaceCare)
            {
                Debug.Log("No front-facing camera found, trying any camera");
                FindCameras(false);
            }
        }
        else
        {
            ErrorEventHandler.InvokeError("Camera Issue:", "Could not find any cameras!", Color.red);
        }
    }
    IEnumerator VerifyWebcamStarted()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (webcam != null)
            {
                Debug.Log($"Webcam playing but checking for valid frame data...");
                webcam.filterMode = FilterMode.Bilinear;
                webcam.Play();

                camView.texture = webcam;

                Debug.Log($"Texture assigned to material: {webcam.width}x{webcam.height}");
                Debug.Log($"Webcam videoRotationAngle: {webcam.videoRotationAngle}");
                Debug.Log($"Webcam videoVerticallyMirrored: {webcam.videoVerticallyMirrored}");
                // Make adjustments to image every frame to be safe, since Unity isn't 
                // guaranteed to report correct data as soon as device camera is started
#if !UNITY_EDITOR
                if (webcam == null || !webcam.isPlaying)
                {
                    bool isCamNull = false;
                    if (webcam == null)
                        isCamNull = true;

                    Debug.Log($"Webcam: {webcam.deviceName} failed to start within timeout. null: {isCamNull} isPlaying: {webcam.isPlaying} didUpdate: {webcam.didUpdateThisFrame}");
                    ErrorEventHandler.InvokeError("Camera Issue:", "Failed to start camera. Try restarting the app. Attempting to reinitialize...", Color.red);

                    yield return new WaitForSeconds(8f);
                    InitWebcam();
                }
#endif
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
    }

    public IEnumerator takeSnap()
    {
        yield return new WaitForEndOfFrame();

        try
        {
            webcam.Pause();

            //takes image directly from camera to get highest quality
            tex = new Texture2D(webcam.width, webcam.height);
            tex.SetPixels(webcam.GetPixels());
            tex.Apply();

            camView.texture = tex;

            uploadPic(tex);
        }
        catch (Exception e)
        {
            Debug.Log(e + "Could not take a photo");
            ErrorEventHandler.InvokeError("Error:", "Could not take a photo! Try Again", Color.red);
            new WaitForSeconds(3);
            uploadPic(tex);
        }
    }

    void uploadPic(Texture2D tex)
    {
        //tex and is assigned a default in inspector

        Texture2D rotatedTex = RotateTexture(tex, false); // true = 90° clockwise
        Texture2D smallerTex = CropAndResize(rotatedTex, 1024, 1024);
        byte[] bytes = smallerTex.EncodeToPNG();
        Debug.Log($"Compressed size: {bytes.Length / 1024f:F2} KB");

        string filename = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
        System.IO.File.WriteAllBytes(path, bytes);

        if (selfieUploader != null)
        {
            Debug.Log("File Upload Coroutine");

            StartCoroutine(selfieUploader.SelfieGetLocation(path));
        }
        if (regManager != null)
        {
            Debug.Log("File Upload Coroutine");

            regManager.CapturedPhotoFinalStep(smallerTex, path);
        }
    }
    public void capturePhoto()
    {
        if (devices.Length > 0)
        {
            if (webcam == null || !webcam.isPlaying)
                InitWebcam();

            overlay.SetActive(false);
            button.SetActive(false);

            StartCoroutine(takeSnap());
        }
        else
        {
            ErrorEventHandler.InvokeError("Camera Issue:", "No Camera detected", Color.red);
        }
    }
    void CleanupWebcam()
    {
        if (webcam != null)
        {
            if (webcam.isPlaying)
                webcam.Stop();

            Destroy(webcam);
            webcam = null;
        }
    }
    void OnDisable()
    {
        CleanupWebcam();
    }
    void OnDestroy()
    {
        CleanupWebcam();
    }
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && (webcam == null || !webcam.isPlaying))
        {
            Debug.Log("App regained focus. Restarting webcam...");
            InitWebcam();
        }
    }

#if UNITY_IOS || UNITY_WEBGL
    private bool CheckPermissionAndRaiseCallbackIfGranted(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (UnityEngine.Application.HasUserAuthorization(authenticationType))
        {
            if (authenticationGrantedAction != null)
                authenticationGrantedAction();

            return true;
        }
        return false;
    }

    private IEnumerator AskForPermissionIfRequired(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
        {
            yield return UnityEngine.Application.RequestUserAuthorization(authenticationType);
            if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
                Debug.Log($"Permission {authenticationType} Denied");
        }
    }
#elif UNITY_ANDROID
    private void PermissionCallbacksPermissionGranted(string permissionName)
    {
        StartCoroutine(DelayedCameraInitialization());
    }

    private IEnumerator DelayedCameraInitialization()
    {
        yield return null;
        InitWebcam();
    }

    private void PermissionCallbacksPermissionDenied(string permissionName)
    {
        Debug.Log($"Permission {permissionName} Denied");
    }

    private void AskCameraPermission()
    {
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += PermissionCallbacksPermissionDenied;
        callbacks.PermissionGranted += PermissionCallbacksPermissionGranted;
        Permission.RequestUserPermission(Permission.Camera, callbacks);
    }
#endif


    Texture2D CropAndResize(Texture2D source, int targetWidth, int targetHeight)
    {
        float sourceAspect = (float)source.width / source.height;
        float targetAspect = (float)targetWidth / targetHeight;

        int cropWidth = source.width;
        int cropHeight = source.height;

        if (sourceAspect > targetAspect)
        {
            // Source is wider than target — crop width
            cropWidth = Mathf.RoundToInt(source.height * targetAspect);
        }
        else
        {
            // Source is taller than target — crop height
            cropHeight = Mathf.RoundToInt(source.width / targetAspect);
        }

        int cropX = (source.width - cropWidth) / 2;
        int cropY = (source.height - cropHeight) / 2;

        // Crop the texture
        Color[] croppedPixels = source.GetPixels(cropX, cropY, cropWidth, cropHeight);
        Texture2D croppedTex = new Texture2D(cropWidth, cropHeight);
        croppedTex.SetPixels(croppedPixels);
        croppedTex.Apply();

        // Resize to target
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(croppedTex, rt);
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    Texture2D RotateTexture(Texture2D original, bool clockwise)
    {
        int width = original.width;
        int height = original.height;
        Texture2D rotated = new Texture2D(height, width);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (clockwise)
                    rotated.SetPixel(y, width - x - 1, original.GetPixel(x, y));
                else
                    rotated.SetPixel(height - y - 1, x, original.GetPixel(x, y));
            }
        }

        rotated.Apply();
        return rotated;
    }
    public void GetHighestAvailableResolution(WebCamDevice device, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (device.availableResolutions != null && device.availableResolutions.Length > 0)
        {
            Resolution highest = device.availableResolutions[0];

            foreach (Resolution res in device.availableResolutions)
            {
                if (res.width * res.height > highest.width * highest.height)
                {
                    highest = res;
                }
            }

            width = highest.width;
            height = highest.height;
        }
        else
        {
            Debug.Log("No available resolutions found for device: " + device.name);
        }
    }
}


