using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class AchievementNotification : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI poitnsNotifText;

    public static GameObject thisInstance;
    void Start()
    {
        thisInstance = gameObject;
        transform.localPosition = new Vector3(0, 2000, 0); // Start off-screen
        StartCoroutine(MoveDown());
    }

    IEnumerator MoveDown()
    {
        transform.DOLocalMoveY(1600, 1.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(3f);
        transform.DOLocalMoveY(3000, 1.2f).SetEase(Ease.OutBack);
    }
    public IEnumerator ConfigureNotif(string header, string body, int points)
    {
        Debug.Log("Achievement Spawned: " + header + " --- " + body);

        bodyText.text = body;
        headerText.text = header;
        poitnsNotifText.text = "You earned " + points + " points!";

        yield return new WaitForSeconds(4f);
        Destroy(thisInstance);
    }
    public void ClickToGoTo()
    {
        Debug.Log("Dissmissing achievement popup");
        transform.DOLocalMoveY(3000, .4f).SetEase(Ease.InQuad);

        //set to right screen and load the scene 
        PlayerPrefs.SetInt(AchievementScreenManager.savedScreenName, 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Achievements");

        StopAllCoroutines();
        Destroy(thisInstance);
    }
}
