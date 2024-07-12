using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour {
	string authStatus;
	string msgReceivingGameObjectName;
	string msgReceivingMethodName;
	double customizedLatitude = 28.5355;
	double customizedLongitude = 77.3910;
	//Button Parameters
	int buttonHeight = 100;
	int buttonWidth = 240;

	//Label Parameters
	int labelHeight = 200;
	int labelWidth = 400;
	// Use this for initialization
	void Start () {
		msgReceivingGameObjectName = "UnityReceiveMessages";
		msgReceivingMethodName = "setLocationCallbackMessage";
		authStatus = "Not Determined";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI()
	{
		if (GUI.Button (new Rect(40, 40, buttonWidth, buttonHeight), "Get Authorization")) {
			int temp = LocationManagerBridge.getAuthrizationLevelForApplication();
			switch (temp) {
				case 0 :
					authStatus = "Not Determined";
					break;
				case 1 :
					authStatus = "Restricted";
					break;
				case 2 :
					authStatus = "Denied";
					break;
				case 3 :
					authStatus = "Authorized Always";
					break;
				case 4 :
					authStatus = "Authorized When In Use";
					break;
				default :
					break;			
			}
		}

		if (GUI.Button (new Rect(300, 40, buttonWidth, buttonHeight), "Request Authorized Always")) {
			LocationManagerBridge.requestAuthorizedAlways();
		}

		if (GUI.Button (new Rect(560, 40, buttonWidth, buttonHeight), "Request Authorized When In Use")) {
				LocationManagerBridge.requestAuthorizedWhenInUse();
		}

		if (GUI.Button (new Rect(820, 40, buttonWidth, buttonHeight), "Show Alert For Permissions")) {
			LocationManagerBridge.showAlertForPermissions("Location services are off", "To use background location you must turn on Location Services Settings", "Settings", "Cancel");
		}

		if (GUI.Button (new Rect(40, 200, buttonWidth, buttonHeight), "Start Location Monitoring")) {
			LocationManagerBridge.setMessageReceivingObjectName( msgReceivingGameObjectName, msgReceivingMethodName);
			LocationManagerBridge.startLocationMonitoring();
		}

		if (GUI.Button (new Rect(300, 200, buttonWidth, buttonHeight), "Stop Location Monitoring")) {
			LocationManagerBridge.stopLocationMonitoring();
		}

		if (GUI.Button (new Rect(40, 360, buttonWidth, buttonHeight), "Set Message Receiving Object Name")) {
			LocationManagerBridge.setMessageReceivingObjectName( msgReceivingGameObjectName, msgReceivingMethodName);
		}

		if (GUI.Button (new Rect(40, 520, buttonWidth, buttonHeight), "Get Address For Current Locationg")) {
			LocationManagerBridge.setMessageReceivingObjectName( msgReceivingGameObjectName, msgReceivingMethodName);
			LocationManagerBridge.getAddressForCurrentLocation();
		}

		if (GUI.Button (new Rect(300, 520, buttonWidth, buttonHeight), "Get Address For Customized Locationg")) {
			LocationManagerBridge.setMessageReceivingObjectName( msgReceivingGameObjectName, msgReceivingMethodName);
			LocationManagerBridge.getAddressForLocationWithLatitudeLongitude(customizedLatitude, customizedLongitude);
		}

		if (GUI.Button (new Rect(40, 680, buttonWidth, buttonHeight), "Open Apple Map With Current Location")) {
			LocationManagerBridge.openAppleMapWithCurrentLocation();
		}

		if (GUI.Button (new Rect(300, 680, buttonWidth, buttonHeight), "Open Apple Map With Custom Location")) {
			LocationManagerBridge.openAppleMapsWithLatitudeLongitude(customizedLatitude, customizedLongitude);
		}

		if (GUI.Button (new Rect(560, 680, buttonWidth, buttonHeight), "Open Apple Map With Driving Mode")) {
			LocationManagerBridge.openAppleMapsWithDrivingMode(customizedLatitude, customizedLongitude);
		}

		if (GUI.Button (new Rect(40, 840, buttonWidth, buttonHeight), "Open Map View With Current Location")) {
			LocationManagerBridge.openMapsViewWithCurrentLocation(100, 400, 300, 300);
		}

		if (GUI.Button (new Rect(300, 840, buttonWidth, buttonHeight), "Open Map View With Custom Location")) {
			LocationManagerBridge.openMapsViewWithSpecificLocation(customizedLatitude, customizedLongitude, 100, 400, 300, 300);
		}

		if (GUI.Button (new Rect(560, 840, buttonWidth, buttonHeight), "Hide Map View")) {
			LocationManagerBridge.showMap(false);
		}

		if (GUI.Button (new Rect(820, 840, buttonWidth, buttonHeight), "Show Map View")) {
			LocationManagerBridge.showMap(true);
		}

		GUI.Label (new Rect (560, 200, labelWidth, labelHeight), "Authrization Status ====  "+authStatus);
		GUI.Label (new Rect (560, 300, labelWidth, labelHeight), "Location === "+UnityReceiveMessages.Instance.getLocation ());
		GUI.Label (new Rect (560, 400, labelWidth, labelHeight), "Address === "+UnityReceiveMessages.Instance.getAddress ());
	}
}
