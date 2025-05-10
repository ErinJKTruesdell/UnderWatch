using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalRequirementManager : MonoBehaviour
{
    //general data
    public TMP_Text dayText;

    //req data
    public GameObject requirementPrefab;
    private TMP_Text reqTitle;
    private TMP_Text reqCompletion;

    private void OnEnable()
    {
        
    }
}
