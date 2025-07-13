using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
public class LocationServiceManager : MonoBehaviour
{
    public static float desiredAccuracyInMeters = 3f;
    public static float updateDistanceInMeters = 3f;

    void OnEnable()
    {
        StartCoroutine(AskPermissions());
    }
    void OnGeoCodeComplete(List<string> premiseNames)
    {
        if (premiseNames.Count > 0)
        {
            foreach (var name in premiseNames)
            {
                Debug.Log("Place name found" + name);
            }
        }
        else
        {
            Debug.Log("No places found.");
        }
    }

    IEnumerator LocationCoroutine()
    {
        Debug.Log("Starting location coroutine...");
        // Start service before querying location
        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);
                
        // Wait until service initializes
        int maxWait = 15;
        while (Input.location.status != LocationServiceStatus.Running && maxWait > 0) {
            Debug.Log("Waiting for location service to initialize");
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }
        // Service didn't initialize in 15 seconds
        if (maxWait < 1)
        {
            Debug.LogFormat("Timed out");
            yield break;
        }
        else
        {
            Debug.LogFormat("Location service live. status {0}", UnityEngine.Input.location.status);
            Debug.LogFormat("Location: "
                + UnityEngine.Input.location.lastData.latitude + " "
                + UnityEngine.Input.location.lastData.longitude + " "
                + UnityEngine.Input.location.lastData.altitude + " "
                + UnityEngine.Input.location.lastData.horizontalAccuracy + " "
                + UnityEngine.Input.location.lastData.timestamp);

            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            float _latitude = Input.location.lastData.latitude;
            float _longitude = Input.location.lastData.longitude;
            
            ConvertCoordinates.StartGeocodeRequest(_latitude, _longitude, OnGeoCodeComplete);
        }
    }

    IEnumerator AskPermissions()
    {
#if UNITY_EDITOR
        Debug.Log("Unity Remote detected, skipping location permission checks.");
        yield return new WaitWhile(() => !UnityEditor.EditorApplication.isRemoteConnected);
#endif
        
#if UNITY_EDITOR
        // No permission handling needed in Editor
#elif UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);
        }
        // First, check if user has location service enabled
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("Android and Location not enabled");
            yield break;
        }

#elif UNITY_IOS
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("IOS and Location not enabled");
            yield break;
        }
#endif
        // Start the location service coroutine
        yield return StartCoroutine(LocationCoroutine());
    }
}
