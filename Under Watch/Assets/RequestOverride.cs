using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RequestOverride : MonoBehaviour
{
    public SelfieUploader SU;

    public void ClickOverride()
    {
        StartCoroutine(RequestOverrideRoutine());
        SU.responseText.text = "Requested override";
        ErrorEventHandler.InvokeError("Override Request Acknowledged!", "Hold for confirmation that your request was sent", Color.yellow);

        SU.CloseBlockerPanel();
    }
    IEnumerator RequestOverrideRoutine()
    {
        Debug.Log("beginning override");
        WWWForm form = new WWWForm();
        form.AddField("username", GameManager.loggedInUser.un);

        using (UnityWebRequest www = UnityWebRequest.Post(GameManager.rootURL + "set_manual_override.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to send manual override: " + www.error);

                ErrorEventHandler.InvokeError("Failed to request override!", "Please email ezk23@drexel.edu to request an override", Color.red);
            }
            else
            {
                string response = www.downloadHandler.text;
                Debug.Log("sent: " + response);

                if (response.Contains("Updated"))
                {
                    ErrorEventHandler.InvokeError("Successfully requested override!", "You will receive points and a new target within 1 day", Color.green);
                }
                else
                {
                    ErrorEventHandler.InvokeError("Failed to request override!", "Please email ezk23@drexel.edu to request an override", Color.red);
                }
            }
        }
    }
}
