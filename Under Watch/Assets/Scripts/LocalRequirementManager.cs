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
    private Queue<IEnumerator> updateQueue = new();
    private bool isProcessingQueue = false;

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
        RequirementEventHandler.LogSubscribers();
        LocalUpdateReqs();

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
        //goNextButton.SetActive(true);
    }
    public void EndDay()
    {
        dayMan.GoToEndDay();
    }
    void LocalUpdateReqs(int reqVal = 1, DayManager.ObjTypes type = DayManager.ObjTypes.other, int overrideVal = -1)
    {        
        //we don't do anything with the arguments' data
        updateQueue.Enqueue(UpdateReqs());
        if (!isProcessingQueue)
            StartCoroutine(ProcessUpdateQueue());
    }
    IEnumerator UpdateReqs()
    {
        yield return StartCoroutine(ClearCurrentReqs());

        localReqDict = new (dayMan.GetCurrentRequirements());
        Debug.Log("Count: " + localReqDict.Count);
        foreach (KeyValuePair<DayManager.ObjTypes, (string name, int progress, int total)> kvp in localReqDict)
        {
            GameObject reqItem = Instantiate(requirementPrefab) as GameObject;
            reqItem.transform.SetParent(gridObj);
            reqItem.transform.localScale = new Vector3(1, 1, 1);

            RequirementItem reqDataObject = reqItem.GetComponent<RequirementItem>();

            (int progress, int total) progressTuple = new(kvp.Value.progress, kvp.Value.total);

            reqDataObject.ConfigureItem(new RequirementObject(kvp.Value.name, progressTuple));
        }
        //these must be IEnumerators to avoid race condition and force rebuild to be called last
        if (DayManager.dayCompleted)
            NextDayAvailable();
        else
            //goNextButton.SetActive(false);

        VLGFiddler.RebuildVLGLayout();
    }
    IEnumerator ClearCurrentReqs()
    {
        Debug.Log("Clearing Req Display");
        //loop backwards to avoid indexing issues
        for (int i = gridObj.childCount - 1; i >= 0; i--)
        {
            Transform child = gridObj.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
        yield return null;

        Debug.Log("Clear completed. Count after: " + gridObj.childCount);
    }
    private IEnumerator ProcessUpdateQueue()
    {
        isProcessingQueue = true;
        while (updateQueue.Count > 0)
        {
            IEnumerator nextReq = updateQueue.Dequeue();
            yield return StartCoroutine(nextReq);
        }
        isProcessingQueue = false;
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
