using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
public class ConvertCoordinates : MonoBehaviour
{
    string apiKey;
    public static ConvertCoordinates instance { get; private set; }

    // Example usage:
    //ConvertCoordinates.StartGeocodeRequest(latitude, longitude, OnGeocodeComplete);
    //void OnGeocodeComplete(List<string> premiseNames) {}

    void Awake()
    {
        apiKey = ApiKeys.GOOGLE_MAPS_KEY;

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public static void StartGeocodeRequest(float latitude, float longitude, Action<List<string>> onComplete)
    {
        Debug.Log($"Starting geocode request for lat: {latitude}, long: {longitude}");
        if (instance != null)
        {
            instance.StartCoroutine(instance.ReverseGeocodeRequest(latitude, longitude, onComplete));
        }
        else
        {
            Debug.Log("GeocodeManager not initialized in scene!");
        }
    }

    public IEnumerator ReverseGeocodeRequest(float latitude, float longitude, Action<List<string>> onComplete)
    {
        Debug.Log("Beginning reverse geocode request...");
        //params:
        //latlng, key
        //option: result_type (premise | street address |subpremise)
        //https://maps.googleapis.com/maps/api/geocode/json?latlng=40.714224,-73.961452&location_type=ROOFTOP&result_type=street_address&key=YOUR_API_KEY
        string coreRequest = "https://maps.googleapis.com/maps/api/geocode/";
        string outputFormat = "json";
        string result_type = "premise"; // Adjust as needed

        string requestURL = $"{coreRequest}{outputFormat}?latlng={latitude},{longitude}&result_type={result_type}&key={apiKey}";
        //Debug.Log($"Request URL: {requestURL}");
        using (UnityWebRequest request = UnityWebRequest.Get(requestURL))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(new List<string>()); // Return empty list on failure
                ErrorEventHandler.InvokeError("Geocoding error: ", $"Request failed: {request.error}", Color.red);
                yield break;
            }

            string json = request.downloadHandler.text;
            var parsedJsonDict = Json.Deserialize(json) as Dictionary<string, object>;

            if (parsedJsonDict.ContainsKey("results"))
            {
                Debug.Log("Results found in geocode response.");
                var results = parsedJsonDict["results"] as List<object>;

                if (results != null && results.Count > 0)
                {
                    var resultDict = results[0] as Dictionary<string, object>;
                    var premiseList = new List<string>();
                    var addressList = new List<string>();

                    ExtractComponent(resultDict, "premise", premiseList);

                    if (premiseList.Count > 0)
                    {
                        onComplete.Invoke(premiseList);
                    }
                    else
                    {
                        ExtractComponent(resultDict, "street_number", addressList);
                        ExtractComponent(resultDict, "route", addressList);

                        if (addressList.Count > 0)
                        {
                            onComplete.Invoke(addressList);
                        }
                    }
                }
                else 
                {
                    Debug.Log("No valid results found in geocode response." );
                    onComplete.Invoke(new List<string>()); // Return empty list if no results
                }
            }
        }
    }

    void ExtractComponent(Dictionary<string, object> resultDict, string resultType, List<string> listToAdd)
    {
        if (resultDict.ContainsKey("address_components"))
        {
            var components = resultDict["address_components"] as List<object>;
            if (components == null) return;

            foreach (var comp in components)
            {
                var compDict = comp as Dictionary<string, object>;
                if (compDict.ContainsKey("types"))
                {
                    var types = compDict["types"] as List<object>;

                    //check various types and add to names
                    if (types != null && types.Contains(resultType))
                    {
                        Debug.Log($"Found {resultType} in geocode response.");
                        string longName = compDict["long_name"] as string;
                        listToAdd.Add(longName);
                    }
                }
            }
        }
    }
}
