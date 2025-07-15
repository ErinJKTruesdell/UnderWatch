using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DisplayTargetProfile : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text fullNameText;
    public TMP_Text locationText;
    public RawImage profilePic;
    public RawImage zoomedProfilePic;

    public TMP_Text workLoc;
    public TMP_Text attributes;

    public GameObject profileInfo;
    public string locationLink;
    public string placeName = "";

    public void ConfigureUser(UserInfo user)
    {
        usernameText.text = "@" + user.un;
        fullNameText.text = user.firstName + " " + user.lastName;
        profilePic.texture = user.profilePic;
        zoomedProfilePic.texture = user.profilePic;

        if (user.un != "")
        {
            StartCoroutine(GetProfileData(user.un));
            StartCoroutine(GetUserLocation(user.un));
        }
        else
        {
            ErrorEventHandler.InvokeError("Target Error", "Can't find your target's username!", Color.red);
        }
    }
    public void ClickOnProfile()
    {
        ShowClickedProfile.userName = usernameText.text;
        ShowClickedProfile.sceneCameFrom = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("ClickedProfile");
    }
    IEnumerator GetProfileData(string un)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", un);
        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-profile-stats.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to load attributes: " + www.error + www.downloadHandler.text);
                ErrorEventHandler.InvokeError("Attribute Server Error:", www.error, Color.red);
                yield break;
            }
            else
            {
                Debug.Log("response: " + www.downloadHandler.text);
                //$reactNum . "|" . $postNum. "|" . $work . "|" . $att1 . "|" . $att2 . "|" . $att3;
                HandleAttributeDisplay(www.downloadHandler.text);
            }
        }
    }

    void HandleAttributeDisplay(string response)
    {
        try
        {
            profileInfo.SetActive(true);

            workLoc.text = "";
            attributes.text = "";

            string[] partition = response.Split("@");
            string[] profAtt = partition[1].Split("|");

            Debug.Log("Assigning attributes" + profAtt[0] + profAtt[1] + profAtt[2] + profAtt[3]);

            //if the work string is empty, don't bother showing the rest
            //check if a string is empty before adding it to the sentence
            if (!string.IsNullOrEmpty(profAtt[0]))
                workLoc.text = "I work at " + profAtt[0];
            else
                throw new Exception("Attributes string is empty.");

            if (!string.IsNullOrEmpty(profAtt[1]))
                attributes.text += $" {profAtt[1]}.";

            if (!string.IsNullOrEmpty(profAtt[2]))
                attributes.text += $" {profAtt[2]}";

            if (!string.IsNullOrEmpty(profAtt[3]))
                attributes.text += $", and {profAtt[3]}!";
        }
        catch (Exception e)
        {
            Debug.Log(e);
            profileInfo.SetActive(false);
            workLoc.text = "";
            attributes.text = "";
        }
        VLGFiddler.RebuildVLGLayout();
    }

    IEnumerator GetUserLocation(string un)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", un);
        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-user-location.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to load level: " + www.error + www.downloadHandler.text);
                ErrorEventHandler.InvokeError("Attribute Server Error:", www.error, Color.red);
                yield break;
            }
            else
            {
                //$latitude ."|". $longitude ."|". $place_name;
                Debug.Log("response: " + www.downloadHandler.text);

                string[] partition = www.downloadHandler.text.Split("|");
                if (partition.Length >= 3)
                {
                    locationLink = "";

                    float latitude = float.Parse(partition[0]);
                    float longitude = float.Parse(partition[1]);
                    string placeName = partition[2];

                    HandleLocationCoords(latitude, longitude);
                    HandlePlaceName(latitude, longitude, placeName);
                }
                else
                {
                    ErrorEventHandler.InvokeError("Incorrect location format!", "Sorry, looks like the location isn't working for this user!", Color.red);
                }
            }
        }
    }

    void HandleLocationCoords(float latitude, float longitude)
    {
        if (latitude != 0 && longitude != 0)
        {
            string coords = latitude + ", " + longitude;    
            if (coords != "0, 0")
            {
#if UNITY_IOS
                locationLink = $"http://maps.apple.com/?daddr={coords}&dirflg=w";
#elif UNITY_ANDROID || UNITY_EDITOR
                locationLink = $"https://www.google.com/maps/dir/?api=1&destination={coords}&travelmode=walking";
#else
                locationLink = "";
#endif
            }
        }
        else
        {
            ErrorEventHandler.InvokeError("Location error!", "The coordinates are invalid", Color.red);
        }
    }

    void HandlePlaceName(float latitude, float longitude, string placeName = "")
    {
        if (placeName == "")
        {
            if (latitude != 0 && longitude != 0)
            {
                Debug.Log("Geocoding coordinates: " + latitude + ", " + longitude);
                ConvertCoordinates.StartGeocodeRequest(latitude, longitude, GeocodePlaceName);
                return;
            }
            else
            {
                ErrorEventHandler.InvokeError("Location error!", "The coordinates are invalid", Color.red);
                placeName = "No location found";
                return;
            }
        }
        else
        {
            locationText.text = placeName;
        }
    }

    void GeocodePlaceName(List<string> locationNames)
    {
        if (locationNames.Count == 1)
        {
            //this means it is a premise
            placeName = locationNames[0];
            Debug.Log("Location found: " + placeName);
            locationText.text = placeName;
        }
        else if (locationNames.Count > 1)
        {
            //this means it is a street address
            placeName = string.Join(", ", locationNames);
            Debug.Log("Location found: " + placeName);
            locationText.text = placeName;
        }
        else
        {
            ErrorEventHandler.InvokeError("Geocoding Error!", "No location found for coordinates", Color.red);
        }
    }

    public void LocationClick()
    {
        Debug.Log("location clicked" + locationLink);
        if (locationLink != "")
        {
            Application.OpenURL(locationLink);
        }
    }
}
