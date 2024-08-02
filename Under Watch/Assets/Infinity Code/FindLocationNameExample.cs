/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example how to find the name of the location by coordinates.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/FindLocationNameExample")]
    public class FindLocationNameExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the map. If not specified, the current instance will be used.
        /// </summary>
        public OnlineMaps map;
        
        /// <summary>
        /// Google API Key
        /// </summary>
        public string googleAPIKey;

        private void Start()
        {
            // If the map is not specified, get the current instance.
            if (map == null) map = OnlineMaps.instance;
            OnMapClick();
            // Subscribe to click event.
            //map.control.OnMapClick += OnMapClick;
        }

        private void OnMapClick()
        {
            // Get the coordinates where the user clicked.
            Vector2 mouseCoords = map.control.GetCoords();

            Vector2 coords = new Vector2(39.9583f, -75.1898f);

            // Try find location name by coordinates.
            OnlineMapsGoogleGeocoding request = new OnlineMapsGoogleGeocoding(coords, googleAPIKey);
            request.Send();
            request.OnComplete += OnRequestComplete;
        }

        private void OnRequestComplete(string s)
        {
            // Show response in console.
            Debug.Log(s);

            OnlineMapsGoogleGeocodingResult[] results = OnlineMapsGoogleGeocoding.GetResults(s);
            if (results.Length > 0) Debug.Log(results[0].formatted_address);
        }
    }
}