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

    public UnityEvent userLoggedIn = new();

    bool isAccountEnabled = true;
    private void Awake()
    {
        //dont destrroy it because it has button hookups!
        RegistrationManager.loginSystem = this;

        //can change any settings inside the init
        DOTween.Init();

        DontDestroyOnLoad(this);
    }
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
            Debug.Log("No cached login data found");
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

    public void loginUponRegister(string username, string email, string password, string firstName, string lastName, Texture pfp)
    {
        Debug.Log("logging in on register: " + username);

        GameManager.loggedInUser = new(username, firstName, lastName, pfp, email);
        isLoggedIn = true;
        StartCoroutine(doTargetAssignment(username));
        //we no longer add points upon registration

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

    public IEnumerator doTargetAssignment(string username)
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

            using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "assignTarget.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    errorMessage = www.error;
                    Debug.Log("unsuccessful assignment!" + errorMessage);
                }
                string responseText = www.downloadHandler.text;

                if (responseText.Contains("New Target"))
                {
                    Debug.Log("successful assignment!" + responseText);
                    SetTargetInfo(responseText);
                }
                else
                {
                    Debug.Log("unsuccessful assignment!" + responseText);
                }
            }
            isWorking = false;
        }
        else
        {
            Debug.Log("not logged in!");
        }
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

        if (isAccountEnabled)
        {
            WWWForm form = new WWWForm();
            form.AddField("email", email);
            form.AddField("password", password);

            using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "login.php", form))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    errorMessage = www.error;

                    Debug.Log("Non-Success Result" + www.error);
                }
                else
                {
                    string responseText = www.downloadHandler.text;
                    if (responseText.Contains("Success"))
                    {
                        isLoggedIn = true;
                        ResetValues();
                        canvasElement.transform.DOLocalMoveY(UnityEngine.Screen.height * 3, .7f).SetEase(Ease.OutQuad).OnComplete(() => gm.ProgressToScene("SocialFeed"));
                        userLoggedIn?.Invoke();

                        SetUserInfo(responseText);

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
        }
        isWorking = false;
        //gm.ProgressToScene("SocialFeed");
    }

    public IEnumerator GetTargetInfo()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-target-info.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Non-Success Result" + www.downloadHandler.text + www.error);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                if (responseText.Contains("Success"))
                {
                    Debug.Log("Target found, saving info");
                    SetTargetInfo(responseText);
                }
                else if (responseText.Contains("No Target Found"))
                {
                    Debug.Log("No target found, reassigning!");
                    StartCoroutine(doTargetAssignment(GameManager.loggedInUser.un));
                }
                else
                {
                    Debug.Log("Error: " + responseText);
                }             
            }
        }
    }

    public void SetTargetInfo(string response)
    {
        try
        {
            //"Success" ."|". $target_name ."|". $targetFirst ."|". $targetLast ."|". $targetPFP;
            string[] dataPartition = response.Split("|");

            string un = dataPartition[1].Trim();
            string firstName = dataPartition[2].Trim();
            string lastName = dataPartition[3].Trim();
            string profilePicURL = GameManager.rootURL + dataPartition[4].Trim();

            GameManager.currTarget = new(un, firstName, lastName);
            StartCoroutine(downloadImageFromURL(profilePicURL, GameManager.currTarget));
        }
        catch (IndexOutOfRangeException e)
        {
            errorText.text = "Response doesn't contain enough parts";
            Debug.LogException(e);
        }
    }

    public void SetUserInfo(string responseText)
    {
        try
        {
            //"Success" . "|" . $username_tmp . "|" .  $email_tmp . "|" . $firstName . "|" . $lastName . "|" . $pfpfURL;
            string[] dataPartition = responseText.Split('|');

            string un = dataPartition[1].Trim();
            string email = dataPartition[2].Trim();
            string firstName = dataPartition[3].Trim();
            string lastName = dataPartition[4].Trim();
            string profilePicURL = GameManager.rootURL + dataPartition[5].Trim();

            GameManager.loggedInUser = new(un, firstName, lastName, _email: email);
            StartCoroutine(downloadImageFromURL(profilePicURL, GameManager.loggedInUser));
            StartCoroutine(GetTargetInfo());
        }
        catch (IndexOutOfRangeException e)
        {
            errorText.text = "Response doesn't contain enough parts";
            Debug.LogException(e);
        }
    }

    IEnumerator CheckAccountEnabled(string email)
    {
        //check if account is disabled on server
        WWWForm form = new WWWForm();
        form.AddField("email", email);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "get-user-enabled.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                errorMessage = www.error;

                Debug.Log("Non-Success Result" + errorMessage);
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
    IEnumerator downloadImageFromURL(string url, UserInfo user)
    {
        Debug.Log("Starting image Download Request for user: " + user.un + " from URL: " + url);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            user.profilePic = ((DownloadHandlerTexture)request.downloadHandler).texture;
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