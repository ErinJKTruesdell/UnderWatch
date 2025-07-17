using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpObject : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public static PopUpObject popUpInstance { get; private set; }
    private void Awake()
    {
        popUpInstance = this;
    }
    public void ConfigurePopup(string _title, string _desc)
    {
        //don't fill a new popup if this is still active
        if (!gameObject.activeSelf)
        {
            title.text = _title;
            desc.text = _desc;

            gameObject.SetActive(true);
        }
    }

    public void CloseAchievementPopUp()
    {
        gameObject.SetActive(false);
    }

}
