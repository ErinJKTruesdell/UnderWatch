using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShowClickedProfile : MonoBehaviour
{
    public GameObject zoomedImage;

    public ProfileDatabase pd;
    public SearchScript searchScript;

    public static string userName;
    public static string sceneCameFrom;

    private void Start()
    {
        pd.fillCanvas(userName, pd.fullNameText.text);

        searchScript = new SearchScript();
    }

    public void hideZoomedImage()
    {
        zoomedImage.SetActive(false);
    }

    public void BackButton()
    {
        SceneManager.LoadScene(sceneCameFrom);
    }
}
