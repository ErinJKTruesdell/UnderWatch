using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class ErrorPopupItem : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI bodyText;

    public static GameObject thisInstance;
    void Start()
    {
        thisInstance = gameObject;
        transform.localPosition = new Vector3(0, 2000, 0); // Start off-screen
        StartCoroutine(MoveDown());
    }

    IEnumerator MoveDown()
    {
        transform.DOLocalMoveY(1700, .4f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(3f);
        transform.DOLocalMoveY(3000, .4f).SetEase(Ease.InQuad);
    }
    public IEnumerator ConfigureErrorPopup(string header, string body, Color? headerColor = null)
    {
        Debug.Log("Error Called: " + header + " --- " + body);
        if (headerColor == null)
            headerColor = Color.black;

        headerText.color = (Color)headerColor;
        bodyText.text = TrimErrorText(body, 250);
        headerText.text = TrimErrorText(header, 100);

        yield return new WaitForSeconds(4f);
        Destroy(thisInstance);
    }

    string TrimErrorText(string text, int maxLength)
    {
        if (text.Length > maxLength)
        {
            text = text.Substring(0, maxLength) + "...";
        }
        return text;
    }
    public void ClickToDismiss()
    {
        Debug.Log("Dissmissing error popup");
        transform.DOLocalMoveY(3000, .4f).SetEase(Ease.InQuad);
        StopAllCoroutines();
        Destroy(thisInstance);
    }
}
