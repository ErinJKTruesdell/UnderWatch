using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class RequirementObject
{
    public string titleText;
    public Tuple<int, int> completionTuple;

    public RequirementObject(string title, Tuple<int, int> completion)
    {
        titleText = title;
        completionTuple = completion;
    }
}

public class LocalRequirementManager : MonoBehaviour
{
    //general data
    public TMP_Text dayText;
    private int dayNum;
    //req data
    public GameObject requirementPrefab;

    //references
    public DayManager dayMan;
    public Dictionary<string, Tuple<int, int>> localReqDict;

    //objs
    public Transform gridObj;
    public GameObject goNextButton;

    private void Awake()
    {
        dayMan = FindObjectOfType<DayManager>();
    }
    private void OnEnable()
    {
        StartCoroutine(UpdateReqs());


        if (DayManager.dayCompleted)
            NextDayAvailable();
        else
            goNextButton.SetActive(false);
        //the daynum update has to go after updateReqs
        dayNum = DayManager.currentDay;
        dayText.text = "Day " + dayNum;

    }
    public void NextDayAvailable()
    {
        goNextButton.SetActive(true);
    }
    public void EndDay()
    {
        dayMan.GoToEndDay();
    }
    IEnumerator UpdateReqs()
    {
        yield return StartCoroutine(ClearCurrentReqs());

        localReqDict = dayMan.GetCurrentRequirements();
        foreach (KeyValuePair<string, Tuple<int, int>> kvp in localReqDict)
        {
            GameObject reqItem = Instantiate(requirementPrefab) as GameObject;
            reqItem.transform.parent = gridObj;
            reqItem.transform.localScale = new Vector3(1, 1, 1);

            RequirementItem reqDataObject = reqItem.GetComponent<RequirementItem>();
            reqDataObject.ConfigureItem(new RequirementObject(kvp.Key, kvp.Value));
        }
        //these must be IEnumerators to avoid race condition and force rebuild to be called last
        VLGFiddler.RebuildVLGLayout();
    }

    IEnumerator ClearCurrentReqs()
    {
        //if we are a day behind, clear the previous day
        if (dayNum != DayManager.currentDay || DayManager.currentDay == 0)
        {
            //loop backwards to avoid indexing issues
            for (int i = gridObj.childCount - 1; i >= 0; i--)
            {
                Transform child = gridObj.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }
        yield return null;
    }
}
