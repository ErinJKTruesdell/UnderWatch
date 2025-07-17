using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowClickedProfile : MonoBehaviour
{
    public GameObject zoomedImage;
    public RawImage zoomedImageTex;

    public ProfileDatabase pd;
    public SearchScript searchScript;

    public static UserInfo user;
    public static string sceneCameFrom;

    private void OnEnable()
    {
        pd.fillCanvas(user);

        searchScript = new SearchScript();
    }

    public void BackButton()
    {
        SceneManager.LoadScene(sceneCameFrom);
    }
}
