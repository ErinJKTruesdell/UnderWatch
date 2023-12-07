using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SC_LoginSystem : MonoBehaviour
{
    public enum CurrentWindow { Login, Register }
    public CurrentWindow currentWindow = CurrentWindow.Login;

    string loginEmail = "";
    string loginPassword = "";
    string registerEmail = "";
    string registerPassword1 = "";
    string registerPassword2 = "";
    string registerUsername = "";
    string errorMessage = "";

    bool isWorking = false;
    bool registrationCompleted = false;
    bool isLoggedIn = false;

    public GameObject LoginButton;
    public GameObject RegisterButton;

    public int registerSceneIndex;

    //Logged-in user data
    string userName = "";
    string userEmail = "";

    string rootURL = "https://erinjktruesdell.com/"; //Path where php files are located

    public bool getIsLoggedIn()
    {
        return isLoggedIn;
    }

    public string getUsername()
    {
        return userName;
    }

    private void Awake()
    {

        DontDestroyOnLoad(this);
        // attempt login with any saved information
        if (PlayerPrefs.GetString("savedUsername", "") != "")
        {

        }
        else
        {
            //else show login/register buttons
            LoginButton.SetActive(true);
            RegisterButton.SetActive(true);
        }
    }

    public void goToRegistration()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(registerSceneIndex);
    }

    void OnGUI()
    {

        


            //if (!isLoggedIn)
            //{
            //    if (currentWindow == CurrentWindow.Login)
            //    {
            //        GUI.Window(0, new Rect(Screen.width / 2 - 250, Screen.height / 2 - 230, 500, 460), LoginWindow, "Login");
            //    }
            //    if (currentWindow == CurrentWindow.Register)
            //    {
            //        GUI.Window(0, new Rect(Screen.width / 2 - 125, Screen.height / 2 - 165, 250, 330), RegisterWindow, "Register");
            //    }
            //}

            //GUI.Label(new Rect(5, 5, 500, 25), "Status: " + (isLoggedIn ? "Logged-in Username: " + userName + " Email: " + userEmail : "Logged-out"));
            //if (isLoggedIn)
            //{
            //    if (GUI.Button(new Rect(5, 30, 100, 25), "Log Out"))
            //    {
            //        isLoggedIn = false;
            //        userName = "";
            //        userEmail = "";
            //        currentWindow = CurrentWindow.Login;
            //    }
            //}
        }

        void LoginWindow(int index)
    {
        if (isWorking)
        {
            GUI.enabled = false;
        }

        if (errorMessage != "")
        {
            GUI.color = Color.red;
            GUILayout.Label(errorMessage);
        }
        if (registrationCompleted)
        {
            GUI.color = Color.green;
            GUILayout.Label("Registration Completed!");
        }

        GUI.color = Color.white;
        GUILayout.Label("Email:");
        loginEmail = GUILayout.TextField(loginEmail);
        GUILayout.Label("Password:");
        loginPassword = GUILayout.PasswordField(loginPassword, '*');

        GUILayout.Space(5);

        if (GUILayout.Button("Submit", GUILayout.Width(85)))
        {
            StartCoroutine(LoginEnumerator());
        }

        GUILayout.FlexibleSpace();

        GUILayout.Label("Do not have account?");
        if (GUILayout.Button("Register", GUILayout.Width(125)))
        {
            ResetValues();
            currentWindow = CurrentWindow.Register;
        }
    }

    void RegisterWindow(int index)
    {
        if (isWorking)
        {
            GUI.enabled = false;
        }

        if (errorMessage != "")
        {
            GUI.color = Color.red;
            GUILayout.Label(errorMessage);
        }

        GUI.color = Color.white;
        GUILayout.Label("Email:");
        registerEmail = GUILayout.TextField(registerEmail, 254);
        GUILayout.Label("Username:");
        registerUsername = GUILayout.TextField(registerUsername, 20);
        GUILayout.Label("Password:");
        registerPassword1 = GUILayout.PasswordField(registerPassword1, '*', 19);
        GUILayout.Label("Password Again:");
        registerPassword2 = GUILayout.PasswordField(registerPassword2, '*', 19);

        GUILayout.Space(5);

        if (GUILayout.Button("Submit", GUILayout.Width(85)))
        {
            StartCoroutine(RegisterEnumerator());
        }

        GUILayout.FlexibleSpace();

        GUILayout.Label("Already have an account?");
        if (GUILayout.Button("Login", GUILayout.Width(125)))
        {
            ResetValues();
            currentWindow = CurrentWindow.Login;
        }
    }

    IEnumerator RegisterEnumerator()
    {
        isWorking = true;
        registrationCompleted = false;
        errorMessage = "";

        WWWForm form = new WWWForm();
        form.AddField("email", registerEmail);
        form.AddField("username", registerUsername);
        form.AddField("password1", registerPassword1);
        form.AddField("password2", registerPassword2);

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "register.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
            }
            else
            {
                string responseText = www.downloadHandler.text;

                if (responseText.StartsWith("Success"))
                {
                    ResetValues();
                    registrationCompleted = true;
                    currentWindow = CurrentWindow.Login;
                }
                else
                {
                    errorMessage = responseText;
                }
            }
        }

        isWorking = false;
    }

    IEnumerator LoginEnumerator()
    {
        isWorking = true;
        registrationCompleted = false;
        errorMessage = "";

        WWWForm form = new WWWForm();
        form.AddField("email", "ejktruesdell@gmail.com");
        form.AddField("password", "nernpass1");

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "login.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
            }
            else
            {
                string responseText = www.downloadHandler.text;

                if (responseText.StartsWith("Success"))
                {
                    string[] dataChunks = responseText.Split('|');
                    userName = dataChunks[1];
                    userEmail = dataChunks[2];
                    isLoggedIn = true;
                    ResetValues();
                }
                else
                {
                    errorMessage = responseText;
                }
            }
        }

        isWorking = false;
        //UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void RecordLocation()
    {
        if (isLoggedIn)
        {
            StartCoroutine(GetAndSendLocationData());
        }
    }

    IEnumerator GetAndSendLocationData()
    {
        Debug.Log("GO GO GO");
        float latitude = 0f;
        float longitude = 0f;
        // get location data
        if (!Input.location.isEnabledByUser)
            Debug.Log("Location not enabled on device or app does not have permission to access location");

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
        }

        // Stops the location service if there is no need to query location updates continuously.
        Input.location.Stop();


        if(latitude != 0)
        {
            isWorking = true;
            errorMessage = "";

            string lat = latitude.ToString();
            string longi = longitude.ToString();

            Debug.Log("Attempting Upload: " + userName + " lat: " + lat + " long: " + longi);

            WWWForm form = new WWWForm();
            form.AddField("username", userName);
            form.AddField("lat", lat);
            form.AddField("long", longi);

            using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "recordLocation.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    errorMessage = www.error;
                }
                else
                {
                    string responseText = www.downloadHandler.text;

                    Debug.Log(responseText);
                }
            }

            isWorking = false;
            //send locatrion data
        }

    }

    void ResetValues()
    {
        errorMessage = "";
        loginEmail = "";
        loginPassword = "";
        registerEmail = "";
        registerPassword1 = "";
        registerPassword2 = "";
        registerUsername = "";
    }
}