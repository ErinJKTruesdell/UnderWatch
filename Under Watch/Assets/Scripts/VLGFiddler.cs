using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VLGFiddler : MonoBehaviour
{
    static RectTransform vlg;
    private void Awake()
    {
        vlg = GetComponent<RectTransform>();
    }
    private void Start()
    {
        //this is necessary, because unity decides to render the VLG BEFORE rendering the elements
        RebuildVLGLayout();
    }

    public static void RebuildVLGLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(vlg);
        Debug.Log("rebuilt");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            RebuildVLGLayout();
        }
    }
}
