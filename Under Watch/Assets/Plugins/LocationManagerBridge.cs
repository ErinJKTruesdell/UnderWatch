using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class LocationManagerBridge {

	[DllImport("__Internal")]
	private static extern int _getAuthrizationLevelForApplication();
	public static int getAuthrizationLevelForApplication() {
		return _getAuthrizationLevelForApplication();
	}

	[DllImport("__Internal")]
	private static extern void _requestAuthorizedAlways();
	public static void requestAuthorizedAlways() {
		_requestAuthorizedAlways();
	}

	[DllImport("__Internal")]
	private static extern void _requestAuthorizedWhenInUse();
	public static void requestAuthorizedWhenInUse() {
		_requestAuthorizedWhenInUse();
	}

	[DllImport("__Internal")]
	private static extern void _showAlertForPermissions(string alertTitle,string alertMessage,string defaultBtnTitle,string cancelBtnTitle);
	public static void showAlertForPermissions(string alertTitle,string alertMessage,string defaultBtnTitle,string cancelBtnTitle) { 
		_showAlertForPermissions(alertTitle, alertMessage, defaultBtnTitle, cancelBtnTitle);
	}

	[DllImport("__Internal")]
	private static extern bool _startLocationMonitoring();
	public static bool startLocationMonitoring() {
		return _startLocationMonitoring();
	}

	[DllImport("__Internal")]
	private static extern void _stopLocationMonitoring();
	public static void stopLocationMonitoring() {
		_stopLocationMonitoring();
	}

	[DllImport("__Internal")]
	private static extern void _setMessageReceivingObjectName(string msgReceivingGameObjectName,string msgReceivingMethodName);
	public static void setMessageReceivingObjectName(string msgReceivingGameObjectName,string msgReceivingMethodName){
		_setMessageReceivingObjectName(msgReceivingGameObjectName, msgReceivingMethodName);
	}

	[DllImport("__Internal")]
	private static extern void _getAddressForCurrentLocation();
	public static void getAddressForCurrentLocation() {
		_getAddressForCurrentLocation();
	}

	[DllImport("__Internal")]
	private static extern void _getAddressForLocationWithLatitudeLongitude(string locationLatitudeTemp, string locationLongitudeTemp);
	public static void getAddressForLocationWithLatitudeLongitude(double locationLatitudeTemp, double locationLongitudeTemp) {
		_getAddressForLocationWithLatitudeLongitude(locationLatitudeTemp.ToString(), locationLongitudeTemp.ToString());
	}

	[DllImport("__Internal")]
	private static extern void _openAppleMapsWithMyCurrentLocation();
	public static void openAppleMapWithCurrentLocation() {
		_openAppleMapsWithMyCurrentLocation();
	}

	[DllImport("__Internal")]
	private static extern void _openAppleMapsWithLatitudeLongitude(string locationLatitudeTemp, string locationLongitudeTemp);
	public static void openAppleMapsWithLatitudeLongitude(double locationLatitudeTemp, double locationLongitudeTemp) {
		_openAppleMapsWithLatitudeLongitude(locationLatitudeTemp.ToString(), locationLongitudeTemp.ToString());
	}

	[DllImport("__Internal")]
	private static extern void _openAppleMapsWithDrivingMode(string locationLatitudeTemp, string locationLongitudeTemp);
	public static void openAppleMapsWithDrivingMode(double locationLatitudeTemp, double locationLongitudeTemp) {
		_openAppleMapsWithDrivingMode(locationLatitudeTemp.ToString(), locationLongitudeTemp.ToString());
	}

	[DllImport("__Internal")]
	private static extern void _openMapsViewWithCurrentLocation(string x_origin, string y_origin,string height, string width);
	public static void openMapsViewWithCurrentLocation(double x_origin, double y_origin,double height, double width) {
		_openMapsViewWithCurrentLocation(x_origin.ToString(), y_origin.ToString(),height.ToString(), width.ToString());
	}

	[DllImport("__Internal")]
	private static extern void _openMapsViewWithSpecificLocation(string locationLatitudeTemp, string locationLongitudeTemp, string x_origin, string y_origin,string height, string width);
	public static void openMapsViewWithSpecificLocation(double locationLatitudeTemp, double locationLongitudeTemp, double x_origin, double y_origin, double height, double width) {
		_openMapsViewWithSpecificLocation(locationLatitudeTemp.ToString(), locationLongitudeTemp.ToString(), x_origin.ToString(), y_origin.ToString(), height.ToString(), width.ToString());
	}


	[DllImport("__Internal")]
	private static extern void _showMap();
	[DllImport("__Internal")]
	private static extern void _hideMap();
	public static void showMap(bool state) {
		if (state) {
			_showMap();
		} else {
			_hideMap();
		}		
	}

}
