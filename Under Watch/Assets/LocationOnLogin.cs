using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LocationOnLogin : MonoBehaviour
{
    string locationName = "";
    float latitude;
    float longitude;

    public IEnumerator GetLocation()
    {
#if !UNITY_EDITOR
        yield return new WaitUntil(() => Input.location.status != LocationServiceStatus.Initializing);

        // get location info for posting
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;

        ConvertCoordinates.StartGeocodeRequest(latitude, longitude, ValidateLocation);
#elif UNITY_EDITOR
        // For testing in the editor, use a fixed location
        latitude = 39.7749f; // Example: San Francisco latitude
        longitude = -79.4194f; // Example: San Francisco longitude
        ConvertCoordinates.StartGeocodeRequest(latitude, longitude, ValidateLocation);
#endif
        locationName = "";

        yield return new WaitForSeconds(.01f);
    }

    void ValidateLocation(List<string> locationNames)
    {
        if (locationNames.Count == 1)
        {
            //this means it is a premise
            locationName = locationNames[0];
            Debug.Log("Location found: " + locationName);
        }
        else if (locationNames.Count > 1)
        {
            //this means it is a street address
            locationName = string.Join(", ", locationNames);
            Debug.Log("Location found: " + locationName);
        }
        else
        {
            ErrorEventHandler.InvokeError("Geocoding Error!", "No location found for coordinates", Color.red);
        }
        StartCoroutine(UploadLocation());
    }

    IEnumerator UploadLocation()
    {
        WWWForm form = new WWWForm();

        form.AddField("username", GameManager.loggedInUser.un);

        form.AddField("latitude", latitude.ToString());
        form.AddField("longitude", longitude.ToString());
        form.AddField("place_name", locationName);

        UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "set-loggedin-location.php", form);
        Debug.Log("Sending web request...");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            ErrorEventHandler.InvokeError("Server Error:", www.error, Color.red);
            Debug.Log(www.error + " " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("result: " + www.result);
            Debug.Log("response: " + www.downloadHandler.text);
        }

    }
}
