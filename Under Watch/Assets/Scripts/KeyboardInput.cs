using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardInput : MonoBehaviour
{
    public TextMeshProUGUI errorText;

    public RectTransform canvasRect;

    public Transform[] elementTransforms;
    public List<Vector3> elementOriginalVectors;
    public void Start()
    {
        foreach (Transform ele in elementTransforms)
        {
            elementOriginalVectors.Add(new Vector3(ele.localPosition.x, ele.localPosition.y, ele.localPosition.z));
        }
    }
    public void OnKeyboardInput()
    {
        errorText.text = TouchScreenKeyboard.area.ToString();

        int heightToSet = 1600;

        foreach (Transform ele in elementTransforms)
        {
            //shifts everything up, this is easily the worst way to do it
            ele.localPosition = new Vector3(ele.localPosition.x, ele.localPosition.y + heightToSet / 8, ele.localPosition.z);
        }
    }

    public void OnKeyboardExit()
    {
        int i = 0;
        foreach (Transform ele in elementTransforms)
        {
            //shifts everything down
            ele.localPosition = elementOriginalVectors[i];
            i++;

            errorText.text = "exit";
        }
    }
}
