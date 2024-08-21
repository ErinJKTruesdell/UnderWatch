using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchInputResizer : MonoBehaviour
{

    public RectTransform scrollView;
    public void OnKeyboardEnter()
    {
        scrollView.sizeDelta = new Vector2(700, 510);
    }

    public void OnKeyboardExit()
    {
        scrollView.sizeDelta = new Vector2(700, 1020);
    }
}
