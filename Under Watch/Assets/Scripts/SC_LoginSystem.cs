using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class SC_LoginSystem : MonoBehaviour
{
    public static SC_LoginSystem sclsInstance;
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
    public bool isLoggedIn = false;

    bool isCached = false;

    public GameObject LoginButton;
    public GameObject RegisterButton;
    public GameObject ForgotPasswordButton;
    public GameObject logo;

    public GameObject canvasElement;

    public Toggle cacheCheckToggle;

    public int registerSceneIndex;

    public PasswordQueryManager pqm;
    public GameManager gm;

    public GameObject loginFields;
    public TMPro.TMP_InputField emailField;
    public TMPro.TMP_InputField pwField;

    public GameObject backButton;

    public TMPro.TMP_Text errorText;

    //Logged-in user data
    public static string userName = "";
    public string userEmail = "";

    public event TargetHandler Target;
    public EventArgs e = null;
    public delegate void TargetHandler(string m, EventArgs e);

    public UnityEvent userLoggedIn = new();

    bool isAccountEnabled = true;
    void Start()
    {
        LoginButton.transform.DOLocalMoveY(-1000, .7f).From().SetEase(Ease.OutQuad);
        RegisterButton.transform.DOLocalMoveY((-1000 - 138f), .7f).From().SetEase(Ease.OutQuad);
        logo.transform.DOLocalMoveY(1000, .7f).From().SetEase(Ease.OutQuad);

        UnityEngine.Application.targetFrameRate = 60; // Or Application.targetFrameRate = Screen.currentResolution.refreshRate;

        // attempt login with any saved information
        if (PlayerPrefs.GetString("savedUsername", "") != "" || PlayerPrefs.GetString("savedPassword", "") != "")
        {
            isCached = true;
            StartCoroutine(LoginEnumerator(PlayerPrefs.GetString("savedUsername", ""), PlayerPrefs.GetString("savedPassword", "")));
            Debug.Log("Cached login data used" + PlayerPrefs.GetString("savedUsername", "") + PlayerPrefs.GetString("savedPassword", ""));
        }
        else
        {
            //else show login/register buttons
            LoginButton.SetActive(true);
            RegisterButton.SetActive(true);
            ForgotPasswordButton.SetActive(false);
        }
    }


    public bool getIsLoggedIn()
    {
        return isLoggedIn;
    }

    public static string getUsername()
    {
        return userName;
    }
    public void loginUponRegister(string username, string email, int points, string password)
    {
        Debug.Log("logging in on register: " + username);
        userName = username;
        userEmail = email;
        isLoggedIn = true;
        StartCoroutine(doTargetAssignment(userName, points));

        userLoggedIn?.Invoke();
    }

    public void showLoginFields()
    {
        if (loginFields != null)
        {
            EnableLoginFields(true);

            loginFields.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad);

            LoginButton.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad);     
            RegisterButton.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad);
            logo.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad).OnComplete(() => EnableLoginReg(false));
        }
    }
    public void backToLogin()
    {
        EnableLoginReg(true);

        backButton.SetActive(false);
        loginFields.transform.DOLocalMoveX(1400f, .5f).SetEase(Ease.OutQuad);

        LoginButton.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad);
        RegisterButton.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutQuad);
        logo.transform.DOLocalMoveX(0f, .5f).SetEase(Ease.OutQuad).OnComplete(() => EnableLoginFields(true));

    }

    void EnableLoginReg(bool enabling)
    {
        LoginButton.SetActive(enabling);
        RegisterButton.SetActive(enabling);
        logo.SetActive(enabling);
    }

    void EnableLoginFields(bool enabling)
    {
//      ForgotPasswordButton.SetActive(enabling);
        backButton.SetActive(enabling);
        loginFields.SetActive(enabling);
    }

    public void tryLogin()
    {
        if(pwField != null && emailField != null)
        {
            if(emailField.text == "")
            {
                errorText.text = "Missing email.";
            } else if (pwField.text == "")
            {
                errorText.text = "Missing password.";
            } else
            {
                StartCoroutine(LoginEnumerator(emailField.text, pwField.text));
            }
        }
    }

    public IEnumerator doTargetAssignment(string username, int pointsToAdd)
    {
        while (isWorking)
        {
            yield return new WaitForSeconds(0.2f);
        }
        isWorking = true;
        if (isLoggedIn)
        {
            Debug.Log("isLoggedIn");

            string errorMessage = "";

            WWWForm form = new WWWForm();
            form.AddField("username", username);
            form.AddField("points", pointsToAdd);

            using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "assignTarget.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    errorMessage = www.error;
                    Debug.Log("unsuccessful assignment!" + errorMessage);
                }
                //else
                // {
                string responseText = www.downloadHandler.text;

                string returnText = "";

                if (responseText.Contains("New Target"))
                {
                    returnText = responseText;
                    Debug.Log("successful assignment!" + returnText);
                }
                else
                {
                    returnText = responseText;
                    Debug.Log("unsuccessful assignment!" + returnText);

                }
                //}
                if (Target != null)
                {
                    Target(returnText, e);
                }
            }

            isWorking = false;
        }
        else
        {
            Debug.Log("not logged in!");
        }
    }

    private void Awake()
    {
        //dont destrroy it because it has button hookups!
        RegistrationManager.loginSystem = this;

        //can change any settings inside the init
        DOTween.Init();

        DontDestroyOnLoad(this);
    }

    public void goToRegistration()
    {
        SceneManager.LoadSceneAsync(registerSceneIndex);
        //wait until the scene fully loads
        LoginButton.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad);
        RegisterButton.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad);
        logo.transform.DOLocalMoveX(-1400f, .5f).SetEase(Ease.OutQuad);
    }

    public void SetLoginPrefs(string email, string password, bool save)
    {
        PlayerPrefs.SetString("savedUsername", email);
        PlayerPrefs.SetString("savedPassword", password);

        PlayerPrefs.Save();
        Debug.Log("User login data successfully cached");
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("User login data cleared");
        }
    }
    public IEnumerator LoginEnumerator(string email, string password)
    {
        isWorking = true;
        registrationCompleted = false;
        errorMessage = "";

        yield return StartCoroutine(CheckAccountEnabled(email));
        if (!isAccountEnabled)
        {
            yield return null;
        }

        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "login.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Non-Success Result");
                errorMessage = www.error;
            }
            else
            {
                string responseText = www.downloadHandler.text;
                if (responseText.Contains("Success"))
                {
                    string[] dataChunks = responseText.Split('|');
                    userName = dataChunks[1];
                    userEmail = dataChunks[2];
                    isLoggedIn = true;
                    ResetValues();
                    gm.saveLoginTime();
                    canvasElement.transform.DOLocalMoveY(UnityEngine.Screen.height * 3, .7f).SetEase(Ease.OutQuad).OnComplete(() => gm.ProgressToScene("SocialFeed"));
                    userLoggedIn?.Invoke();

                    //store registration information 
                    if (isCached == false)
                    {
                       Debug.Log(isCached);
                       SetLoginPrefs(email, password, true);
                    }
                }
                else
                {
                    errorMessage = responseText;
                    errorText.text = errorMessage;
                    Debug.Log(errorMessage);
                }
            }
        }

        isWorking = false;
        //gm.ProgressToScene("SocialFeed");
    }
    IEnumerator CheckAccountEnabled(string email)
    {
        //check if account is disabled on server
        WWWForm form = new WWWForm();
        form.AddField("email", email);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "CHANGE ME CHANGE ME", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Non-Success Result");
                errorMessage = www.error;
            }
            else
            {
                string responseText = www.downloadHandler.text;
                if (responseText.Contains("Disabled"))
                {
                    errorMessage = "Account is disabled! Email the SnapGram team if this was a mistake.";
                    errorText.text = errorMessage;
                    Debug.Log(errorMessage);

                    isAccountEnabled = false;
                }
                else if (responseText.Contains("Enabled"))
                {
                    Debug.Log("account is enabled, proceeding");
                    isAccountEnabled = true;
                }
                else
                {
                    errorMessage = responseText;
                    errorText.text = errorMessage;
                    Debug.Log(errorMessage);
                }
            }
        }
    }

    public IEnumerator sendResetRequest(string email)
    {
        isWorking = true;
        errorMessage = "";

        WWWForm form = new WWWForm();
        form.AddField("email", email);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "pwd_reset_query.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
                Debug.Log(www.error);
            }

            string responseText = www.downloadHandler.text;
            Debug.Log(responseText);

            if (pqm != null)
            {
                pqm.receiveQueryResponse(responseText);
            }

        }

        isWorking = false;
    }

    public IEnumerator sendResetUpdatePassword(string email, string code, string newpw)
    {
        isWorking = true;
        errorMessage = "";

        WWWForm form = new WWWForm();
        form.AddField("email", email);

        form.AddField("new_pw", newpw);

        form.AddField("reset_code", code);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "pwd_reset_action.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;
            }

            string responseText = www.downloadHandler.text;

            Debug.Log(responseText);
            if (pqm != null)
            {
                pqm.receiveResetResponse(responseText);
            }

        }

        isWorking = false;
    }

    /*IEnumerator GetAndSendLocationData()
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


        if (latitude != 0)
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

            using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "recordLocation.php", form))
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

    }*/

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