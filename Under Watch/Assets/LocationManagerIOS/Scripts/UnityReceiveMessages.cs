using UnityEngine;
using System.Collections;
using System.IO;
public class UnityReceiveMessages : MonoBehaviour {
	public static UnityReceiveMessages Instance;
	string location;
	string address;

	void Awake(){
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		location = "";
		address = "";
	}

	// Update is called once per frame
	void Update () {
	
	}
	
	public void setLocationCallbackMessage(string messageTemp) {
		string[] values = messageTemp.Split('/');
		if (values.Length > 1) {
			string finalMsg = "";
			if (values[0] == "Address" || values[0] == "AddressError") {
				for (int i = 1; i < values.Length; i++) {
					finalMsg = finalMsg + values[i];
				}
				address = finalMsg;
			} else if (values[0] == "LocationError" || values[0] == "Location") {
				for (int i = 1; i < values.Length; i++) {
					finalMsg = finalMsg + values[i];
				}
				location = finalMsg;
			} 
		}
		
	}

	public string getLocation() {
		return location;
	}

	public string getAddress() {
		return address;
	}
}
