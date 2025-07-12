using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class ErrorManager : MonoBehaviour
{
    public static ErrorManager errorInstance;
    public GameObject errorPrefab;
    public Transform errorParent;

    private Queue<IEnumerator> errorQueue = new();
    bool isProcessingQueue = false;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (errorInstance == null)
        {
            errorInstance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        ErrorEventHandler.onHitAnError += EnqueueError;
    }
    void OnDisable()
    {
        ErrorEventHandler.onHitAnError -= EnqueueError;
    }
    void EnqueueError(string header, string body, Color? headerColor)
    {
        errorQueue.Enqueue(CoroutineDisplayError(header, body, headerColor));
        if (!isProcessingQueue)
            StartCoroutine(ProcessErrorQueue());
    }
    private IEnumerator ProcessErrorQueue()
    {
        isProcessingQueue = true;
        while (errorQueue.Count > 0)
        {
            IEnumerator nextError = errorQueue.Dequeue();
            yield return StartCoroutine(nextError);
        }
        isProcessingQueue = false;
    }
    IEnumerator CoroutineDisplayError(string header, string body, Color? headerColor)
    {
        GameObject error = Instantiate(errorPrefab, errorParent.transform);
        ErrorPopupItem errorPopup = errorPrefab.GetComponent<ErrorPopupItem>();
        yield return StartCoroutine(errorPopup.ConfigureErrorPopup(header, body, headerColor));
    }   
}
