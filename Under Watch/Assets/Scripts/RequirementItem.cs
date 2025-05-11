using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RequirementItem : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text completionText;
    public GameObject checkImage;
    public GameObject crossImage;

    //configitem is called everytime the LocalRequirementManager is enabled
    public void ConfigureItem(RequirementObject reqObj)
    {
        titleText.text = "\u2022<indent=1em>" + reqObj.titleText;
        int progress = reqObj.completionTuple.progress;
        int total = reqObj.completionTuple.total;

        if (progress >= total)
        {
            completionText.text = $"{progress} / {total}";
            checkImage.SetActive(true);
            crossImage.SetActive(true);
        }
        else
        {
            completionText.text = $"{progress} / {total}";
        }
    }
}
