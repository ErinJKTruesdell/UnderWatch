using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_Text errorText;

    public TMP_InputField username;
    public TMP_InputField password;
    private string loggedInUser { get; set; }


    string rootURL = "https://erinjktruesdell.com/";

    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        LoginManager other = GameObject.FindObjectOfType<LoginManager>();
        if(other != null)
        {
            Destroy(gameObject);
        }
    }
    public void tryLogin()
    {
        if (password != null && username != null)
        {
            if (username.text == "")
            {
                errorText.text = "Missing email.";
            }
            else if (password.text == "")
            {
                errorText.text = "Missing password.";
            }
            else
            {
                StartCoroutine(LoginEnumerator(username.text, password.text));
            }
        }
    }

    public IEnumerator LoginEnumerator(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", email);
        form.AddField("password1", password);

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "admin-login.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Non-Success Result");
                errorText.text = www.error;
            }
            else
            {
                string responseText = www.downloadHandler.text;
                if (responseText.StartsWith("Success"))
                {
                    loggedInUser = email;
                     SceneManager.LoadScene(1);
                }
                else
                {
                    errorText.text = responseText;
                }
            }
        }

        //gm.ProgressToScene("SocialFeed");
    }
}
