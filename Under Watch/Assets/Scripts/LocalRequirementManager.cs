using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class RequirementObject
{
    public string titleText;
    public (int progress, int total) completionTuple;

    public RequirementObject(string title, (int progress, int total) completion)
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
    public Dictionary<DayManager.ObjTypes, (string name, int progress, int total)> localReqDict;

    //objs
    public Transform gridObj;
    public GameObject goNextButton;

    private void Awake()
    {
        dayMan = FindObjectOfType<DayManager>();
    }
    private void OnEnable()
    {
        RequirementEventHandler.OnCompletedAReq += LocalUpdateReqs;
        LocalUpdateReqs(DayManager.ObjTypes.other, 0);

        if (DayManager.dayCompleted)
            NextDayAvailable();
        else
            goNextButton.SetActive(false);
        //the daynum update has to go after updateReqs
        dayNum = DayManager.currentDay;
        dayText.text = "Day " + dayNum;
    }
    private void OnDisable()
    {
        RequirementEventHandler.OnCompletedAReq -= LocalUpdateReqs;
    }
    public void NextDayAvailable()
    {
        goNextButton.SetActive(true);
    }
    public void EndDay()
    {
        dayMan.GoToEndDay();
    }
    void LocalUpdateReqs(DayManager.ObjTypes objTypes, int valueToAdd)
    {
        //we don't do anything with the arguments' data
        StartCoroutine(UpdateReqs(objTypes, valueToAdd));
    }
    IEnumerator UpdateReqs(DayManager.ObjTypes objTypes, int valueToAdd)
    {
        yield return StartCoroutine(ClearCurrentReqs());

        localReqDict = dayMan.GetCurrentRequirements();
        Debug.Log("Count: " + localReqDict.Count);
        foreach (KeyValuePair<DayManager.ObjTypes, (string name, int progress, int total)> kvp in localReqDict)
        {
            GameObject reqItem = Instantiate(requirementPrefab) as GameObject;
            reqItem.transform.parent = gridObj;
            reqItem.transform.localScale = new Vector3(1, 1, 1);

            RequirementItem reqDataObject = reqItem.GetComponent<RequirementItem>();

            (int progress, int total) progressTuple = new(kvp.Value.progress, kvp.Value.total);

            reqDataObject.ConfigureItem(new RequirementObject(kvp.Value.name, progressTuple));
        }
        //these must be IEnumerators to avoid race condition and force rebuild to be called last
        VLGFiddler.RebuildVLGLayout();
    }

    IEnumerator ClearCurrentReqs()
    {
        //if we are a day behind, clear the previous day
        if (dayNum != DayManager.currentDay || DayManager.currentDay == 0)
        {
            Debug.Log("Clearing Req Display");
            //loop backwards to avoid indexing issues
            for (int i = gridObj.childCount - 1; i >= 0; i--)
            {
                Transform child = gridObj.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }
        yield return null;

        Debug.Log("Clear completed. Count after: " + gridObj.childCount);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.register);
            RequirementEventHandler.InvokeAddToReq(1, DayManager.ObjTypes.profilePic);
            Debug.Log("skipping register and pfp");
        }
    }
}
